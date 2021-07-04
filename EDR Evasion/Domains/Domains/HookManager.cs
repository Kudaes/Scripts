using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Domains
{
    public class HookManager
    {

        private static List<byte[]> originalOpcodes;
        private static IntPtr libraryAddress;
        private string libraryName;
        private string[] functionsNames;
        private static IntPtr[] addresses;
        private int hookLength;

        //Remove!!
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate IntPtr LoadLibrary_Delegate(string lpFileName);

        public IntPtr hookFunc(string lpFileName)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(lpFileName);
            var str = System.Text.Encoding.Default.GetString(bytes);
            Console.WriteLine("[!] Dll loaded: " + str);


            Uninstall();
            var r = API.LoadLibrary(str);
            Install();
            return r;
        }

        public HookManager()
        {
            libraryName = @"c:\windows\system32\kernel32.dll";
            functionsNames = new string[] { "GetFileAttributesW", "GetFileAttributesExW", "CreateFileW", "GetFileInformationByHandle" };
            originalOpcodes = new List<byte[]>();
            hookLength = is64BitsProcessor() ? 13 : 6;
            libraryAddress = API.LoadLibrary(libraryName);
            addresses = new IntPtr[] { API.GetProcAddress(libraryAddress, functionsNames[0]), API.GetProcAddress(libraryAddress, functionsNames[1]),
                                       API.GetProcAddress(libraryAddress, functionsNames[2]), API.GetProcAddress(libraryAddress, functionsNames[3])};

        }
        private bool is64BitsProcessor()
        {
            return IntPtr.Size == 8 ? true : false;
        }

        public bool Install()
        {
            int c = 0;
            foreach (var address in addresses)
            {
                if (address == IntPtr.Zero)
                    return false;

                bool response = API.VirtualProtectEx((IntPtr)(-1), address, (UIntPtr)hookLength, (uint)0x04, out uint oldProtect);

                if (!response)
                    return false;


                Delegate d;
                switch (c)
                {
                    case 0:
                        d = (API.DELEGATES.GetFileAttributesW)TransactedAssembly.GetFileAttributesWDetour;
                        break;
                    case 1:
                        d = (API.DELEGATES.GetFileAttributesExW)TransactedAssembly.GetFileAttributesExWDetour;
                        break;
                    case 2:
                        d = (API.DELEGATES.CreateFileW)TransactedAssembly.CreateFileWDetour;
                        break;
                    case 3:
                        d = (API.DELEGATES.GetFileInformationByHandle)TransactedAssembly.GetFileInformationByHandleDetour;
                        break;
                    default: return false;
                }

                IntPtr replacementSite = Marshal.GetFunctionPointerForDelegate(d);

                unsafe
                {
                    byte[] ogOpcodes = new byte[hookLength];
                    byte* originalSitePointer = (byte*)address.ToPointer();

                    for (int k = 0; k < hookLength; k++)
                    {
                        ogOpcodes[k] = *(originalSitePointer + k);
                    }

                    originalOpcodes.Add(ogOpcodes);

                    if (is64BitsProcessor())
                    {

                        *originalSitePointer = 0x49;
                        *(originalSitePointer + 1) = 0xBB;
                        *((ulong*)(originalSitePointer + 2)) = (ulong)replacementSite.ToInt64(); //sets 8 bytes

                        //jmp r11
                        *(originalSitePointer + 10) = 0x41;
                        *(originalSitePointer + 11) = 0xFF;
                        *(originalSitePointer + 12) = 0xE3;
                    }
                    else
                    {

                        *originalSitePointer = 0x68;
                        *((uint*)(originalSitePointer + 1)) = (uint)replacementSite.ToInt32(); //sets 4 bytes

                        //ret
                        *(originalSitePointer + 5) = 0xC3;
                    }

                }
                response = API.VirtualProtectEx((IntPtr)(-1), address, (UIntPtr)hookLength, oldProtect, out uint _);

                if (!response)
                    return false;

                c++;
            }

            return true;

        }

        public unsafe bool Uninstall()
        {

            foreach (var address in addresses)
            {
                bool response = API.VirtualProtectEx((IntPtr)(-1), address, (UIntPtr)13, (uint)0x40, out uint oldProtect);

                if (!response)
                    return false;

                byte* originalSitePointer = (byte*)address.ToPointer();

                byte[] ogOpCodes = originalOpcodes[0];
                originalOpcodes.RemoveAt(0);

                for (int k = 0; k < 13; k++)
                    *(originalSitePointer + k) = ogOpCodes[k];

                response = API.VirtualProtectEx((IntPtr)(-1), address, (UIntPtr)hookLength, oldProtect, out uint _);

                if (!response)
                    return false;
            }

            return true;
        }

    }
}
