using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace ExportedFunctions
{
    class HookManager
    {

        private static List<string> functionsName;
        private static List<string> dllsName;
        private static List<string> firstBytes;
        private static int number;

        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymInitialize(IntPtr hProcess, string UserSearchPath, [MarshalAs(UnmanagedType.Bool)]bool fInvadeProcess);

        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymCleanup(IntPtr hProcess);

        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern ulong SymLoadModuleEx(IntPtr hProcess,
                                                   IntPtr hFile,
                                                   string ImageName,
                                                   string ModuleName,
                                                   long BaseOfDll,
                                                   int DllSize,
                                                   IntPtr Data,
                                                   int Flags);


        [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymEnumerateSymbols64(IntPtr hProcess,
                                                        ulong BaseOfDll,
                                                        SymEnumerateSymbolsProc64 EnumSymbolsCallback,
                                                        IntPtr UserContext);

        public delegate bool SymEnumerateSymbolsProc64(string SymbolName,
                                                       ulong SymbolAddress,
                                                       uint SymbolSize,
                                                       IntPtr UserContext);
        
        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        public static bool EnumSyms(string name, ulong address, uint size, IntPtr context)
        {
            if(name != "")
                functionsName[functionsName.Count - 1] += name + ";";
            return true;
        }      

        public HookManager(int n)
        {
            dllsName = new List<string>();
            functionsName = new List<string>();
            firstBytes = new List<string>();
            number = n;
        }

        public void enumExportedFunctions()
        {
            IntPtr hCurrentProcess = (IntPtr)(-1);

            bool status = SymInitialize(hCurrentProcess, null, false);

            if (status == false)
            {
                Console.Out.WriteLine("Failed to initialize sym.");
                return;
            }

            Process p = Process.GetCurrentProcess();

            foreach (ProcessModule m in p.Modules)
            {
                if (!m.FileName.Contains(".exe"))
                {

                    long baseOfDll2 = (long)LoadLibrary(m.FileName);

                    // Load dll.
                    ulong baseOfDll = SymLoadModuleEx(hCurrentProcess,
                                                      IntPtr.Zero,
                                                      m.FileName,
                                                      null,
                                                      baseOfDll2,
                                                      0,
                                                      IntPtr.Zero,
                                                      0);


                    if (baseOfDll != 0)
                    {
                        dllsName.Add(m.FileName);
                        functionsName.Add("");

                        if (SymEnumerateSymbols64(hCurrentProcess, baseOfDll, EnumSyms, IntPtr.Zero) == false)
                        {
                            Console.Out.WriteLine("Failed to enum symbols.");

                        }

                        if(functionsName[functionsName.Count - 1] == "")
                        {
                            dllsName.RemoveAt(dllsName.Count - 1);
                            functionsName.RemoveAt(functionsName.Count - 1);
                        }
                    }
                }
            }


            // Cleanup.
            SymCleanup(hCurrentProcess);
        }


        public void getFirstBytes(string chosen, string filepath)
        {

            if (chosen != null)
                parseFunctions(chosen);

            for(int i = 0; i < dllsName.Count; i++)
            {

                firstBytes.Add("");
                var dll = dllsName[i];
                var functions = functionsName[i].Split(';');
                foreach (var f in functions)
                {
                    IntPtr ptr = GetProcAddress(LoadLibrary(dll), f);
                    if (ptr != IntPtr.Zero)
                    {
                        unsafe
                        {
                            byte* opCode = (byte*)ptr.ToPointer();
                            byte[] originalOpCodes = new byte[number]; // 6 bytes?

                           
                            for (int k = 0; k < number; k++)
                                originalOpCodes[k] = *(opCode + k);

                            var hex = BitConverter.ToString(originalOpCodes).Replace("-", "");
                            

                            firstBytes[i] += hex + ";";
                        }
                    }
                    else
                        firstBytes[i] += "000000000000;";
                    
                }

                if (firstBytes[firstBytes.Count - 1] == "")
                {
                    dllsName.RemoveAt(firstBytes.Count - 1);
                    functionsName.RemoveAt(firstBytes.Count - 1);
                    firstBytes.RemoveAt(firstBytes.Count - 1);
                }
            }

            if(filepath != null)
                writeBytesToFile(filepath);
            else
                printBytes();
            
        }

        public void checkPatch(string filepath, string funcs, string b)
        {
            List<string> dlls = null;
            List<string> ffs = null;

            if (filepath != null)
            {
                readBytesfromFile(filepath);
                if (funcs != null)
                    parseFunctionsB(funcs, ref dlls, ref ffs);
            }
            else
            {
                if (funcs != null && b!= null)
                {
                    parseFunctions(funcs);
                    parseBytes(b);

                }
                else
                {
                    Console.WriteLine("Not enough arguments.");
                    return;
                }
            }

            int hooked = 0;

            for (int i = 0; i < dllsName.Count; i++)
            {
                var dll = dllsName[i];
                if (validDll(dll, dlls))
                {
                    var functions = functionsName[i].Split(';');
                    int count = 0;
                    var bytes = firstBytes[i].Split(';');
                    foreach (var f in functions)
                    {
                        if (validFunc(f, ffs))
                        {
                            IntPtr ptr = GetProcAddress(LoadLibrary(dll), f);
                            if (ptr != IntPtr.Zero)
                            {
                                unsafe
                                {
                                    byte* opCode = (byte*)ptr.ToPointer();// real
                                    var original = bytes[count];

                                    if (original != "000000000000")
                                    {
                                        int NumberChars = original.Length;
                                        byte[] originalOpCodes = new byte[NumberChars / 2];
                                        for (int l = 0; l < NumberChars; l += 2)
                                            originalOpCodes[l / 2] = Convert.ToByte(original.Substring(l, 2), 16);

                                        var c = false;
                                        byte[] copy = new byte[NumberChars / 2];

                                        for (int k = 0; k < number; k++)
                                        {
                                            copy[k] = *(opCode + k);
                                            if (originalOpCodes[k] != copy[k])
                                            {
                                                if (c) // Hay algunas funciones cuyo primer byte baila, pero el resto se mantiene estable.
                                                {
                                                    var hex = BitConverter.ToString(copy).Replace("-", "");
                                                    Console.WriteLine("Hookeada la funcion " + f + " de la dll " + dll + " " + original + " - " + hex);
                                                    hooked++;
                                                    break;
                                                }
                                                else
                                                    c = true;
                                            }

                                        }
                                    }
                                }
                            }
                        }

                        count++;
                    }
                }
            }

            Console.WriteLine("Hookeadas un total de " + hooked + " funciones.");

        }

        public void unhook(string filepath, string funcs, string b)
        {
            List<string> dlls = null;
            List<string> ffs = null;

            if (filepath != null)
            {
                readBytesfromFile(filepath);
                if (funcs != null)
                    parseFunctionsB(funcs, ref dlls, ref ffs);
            }
            else
            {
                if (funcs != null && b != null)
                {
                    parseFunctions(funcs);
                    parseBytes(b);

                }
                else
                {
                    Console.WriteLine("Not enough arguments.");
                    return;
                }
            }

            int unhooked = 0;

            for (int i = 0; i < dllsName.Count; i++)
            {
                var dll = dllsName[i];
                if (validDll(dll, dlls))
                {
                    var functions = functionsName[i].Split(';');
                    int count = 0;
                    var bytes = firstBytes[i].Split(';');
                    foreach (var f in functions)
                    {
                        if (validFunc(f, ffs))
                        {
                            IntPtr ptr = GetProcAddress(LoadLibrary(dll), f);
                            if (ptr != IntPtr.Zero)
                            {
                                unsafe
                                {
                                    byte* opCode = (byte*)ptr.ToPointer();
                                    var original = bytes[count];

                                    if (original != "000000000000")
                                    {
                                        int NumberChars = original.Length;
                                        byte[] originalOpCodes = new byte[NumberChars / 2];
                                        for (int l = 0; l < NumberChars; l += 2)
                                            originalOpCodes[l / 2] = Convert.ToByte(original.Substring(l, 2), 16);

                                        var c = false;
                                        byte[] copy = new byte[NumberChars / 2];

                                        VirtualProtectEx((IntPtr)(-1), ptr, (UIntPtr)number, 0x04, out uint oldProtect);

                                        for (int k = 0; k < number; k++)
                                            *(opCode + k) = originalOpCodes[k];

                                        VirtualProtectEx((IntPtr)(-1), ptr, (UIntPtr)number, oldProtect, out var _);

                                        unhooked++;
                                    }
                                }
                            }
                        }

                        count++;
                    }
                }
            }

            Console.WriteLine("Parcheadas un total de " + unhooked + " funciones.");

        }


        private void printBytes()
        {
            var count = 0;
            foreach (var dll in dllsName)
            {
                var count2 = 0;
                foreach (var func in functionsName)
                {
                    var spl1 = func.Split(';');
                    var spl2 = firstBytes[count].Split(';');
                    Console.WriteLine("[*] " + dll + "!" + spl1[count2] + ": " + spl2[count2]);
                    count2++;
                }
                count++;
            }
        }

        private bool validDll(string dll, List<string> selected)
        {
            if (selected == null)
                return true;

            foreach(var s in selected)
                if(dll.ToLower().Contains(s.ToLower()))
                    return true;

            return false;
        }

        private bool validFunc(string func, List<string> selected)
        {
            if (selected == null)
                return true;

            foreach (var s in selected)
                if (func == s)
                    return true;

            return false;
        }

        private void writeBytesToFile(string filepath)
        {
            string s = "";
            for (int i = 0; i < dllsName.Count; i++)
            {
                s += dllsName[i] + '%';
                s += functionsName[i];

                if (i < (dllsName.Count - 1))
                    s += "|";
            }

            s += '*';

            foreach (var b in firstBytes)
                s += b + '|';

            using (StreamWriter sw = File.CreateText(filepath))
            {
                sw.WriteLine(s);
            }
        }

        private void readBytesfromFile(string filepath)
        {

            functionsName = new List<string>();
            dllsName = new List<string>();
            firstBytes = new List<string>();

            string content = File.ReadAllText(filepath);

            var spl1 = content.Split('*');
            var bytes = spl1[1].Split('|');

            foreach (var b in bytes)
                firstBytes.Add(b);

            firstBytes.RemoveAt(firstBytes.Count - 1);

            var spl2 = spl1[0].Split('|');

            foreach (var s in spl2)
            {
                var c = s.Split('%');
                dllsName.Add(c[0]);
                functionsName.Add(c[1]);
            }
        }

        /*
         * 
         * amsi.dll>AmsiScanBuffer;OtherBuffer&kernel32.dll>LoadLibraryA;Advapi32.dll...
         * 
         */
        private void parseFunctions(string f)
        {
            dllsName = new List<string>();
            functionsName = new List<string>();

            var spl = f.Split('&');
            
            foreach(var s in spl)
            {
                var spl2 = s.Split('>');
                dllsName.Add(spl2[0]);
                functionsName.Add(spl2[1]);
            }
        }

        private void parseFunctionsB(string f, ref List<string> dlls, ref List<string> functions)
        {
            dlls = new List<string>();
            functions = new List<string>();

            var spl = f.Split('&');

            foreach (var s in spl)
            {
                var spl2 = s.Split('>');
                dlls.Add(spl2[0]);
                functions.Add(spl2[1]);
            }
        }

        private void parseBytes(string b)
        {
            firstBytes = new List<string>();

            var spl = b.Split('&');

            foreach (var s in spl)
                firstBytes.Add(s);
            
        }
    }
}
