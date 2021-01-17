using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Injector
{
    public class HookManager
    {

        public static byte[] originalOpcodes;
        public static byte[] originalSh;
        public static IntPtr libraryAddress;
        public string libraryName;
        public string functionName;
        public static IntPtr address;
        public static IntPtr scAddr;
        public static int scSize;

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate IntPtr GetProcAddress_Delegate(IntPtr hModule, string procName);

        public IntPtr hookFunc(IntPtr hModule, string procName)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(procName);
            var str = Encoding.Default.GetString(bytes);

            Console.WriteLine("[!] Function called: " + str);

            IntPtr r = IntPtr.Zero;
            try
            {
                Uninstall();
                r = GetProcAddress(hModule, str);
 //             Thread.Sleep(200);
                Install();
            }
            catch { }

           
            return r;
        }
        public HookManager()
        {
            libraryName = @"c:\windows\system32\kernelbase.dll";
            functionName = "GetProcAddress";
            originalOpcodes = is64BitsProcessor() ? new byte[13] : new byte[6];
            originalSh = null;
            libraryAddress = LoadLibrary(libraryName);
            address = GetProcAddress(libraryAddress, functionName);
            scAddr = IntPtr.Zero;
            scSize = 0;
        }
        private bool is64BitsProcessor()
        {
            return IntPtr.Size == 8 ? true : false;
        }

        public void load(string dll)
        {
            LoadLibrary(dll);
        }

        public bool Install()
        {

            if (address == IntPtr.Zero)
                return false;

            bool response = VirtualProtectEx((IntPtr)(-1), address, (UIntPtr)13, (uint)0x04, out uint oldProtect);

            if (!response)
                 return false;

            GetProcAddress_Delegate d = (GetProcAddress_Delegate)hookFunc;

            IntPtr replacementSite = Marshal.GetFunctionPointerForDelegate(d);

            unsafe
            {
                byte* originalSitePointer = (byte*)address.ToPointer();

                for (int k = 0; k < originalOpcodes.Length; k++)
                {
                    originalOpcodes[k] = *(originalSitePointer + k);
                }

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
            response = VirtualProtectEx((IntPtr)(-1), address, (UIntPtr)13, oldProtect, out uint _);

            if (!response)
                return false;

            return true;
        }
        

        public unsafe bool unhookSyscall(string dllName, string apiCall, byte[] content)
        {

            libraryAddress = LoadLibrary(libraryName);
            address = GetProcAddress(libraryAddress, functionName);

            if (address == IntPtr.Zero)
                return false;

            bool response = VirtualProtectEx((IntPtr)(-1), address, (UIntPtr)13, (uint)0x40, out uint oldProtect);

            if (!response)
                return false;

            byte* originalSitePointer = (byte*)address.ToPointer();

            for(int k = 0; k < content.Length; k++)
                *(originalSitePointer + k) = content[k];

            return true;
        }

        public unsafe bool Uninstall()
        {


            bool response = VirtualProtectEx((IntPtr)(-1), address, (UIntPtr)13, (uint)0x40, out uint oldProtect);

            if (!response)
                return false;

            byte* originalSitePointer = (byte*)address.ToPointer();

            for (int k = 0; k < 13; k++)
                *(originalSitePointer + k) = originalOpcodes[k];

            response = VirtualProtectEx((IntPtr)(-1), address, (UIntPtr)13, oldProtect, out uint _);

            if (!response)
                return false;
            return true;
        }

        public unsafe void setScInfo(IntPtr addr, int size)
        {
            scAddr = addr;
            scSize = size;
            var length = size / 3;
            originalSh = new byte[size];
            byte* originalSitePointer = (byte*)addr.ToPointer();

            for (int k = 0; k < size; k++)
            {
                originalSh[k] = *(originalSitePointer + k);
            }
        }
    }
}
