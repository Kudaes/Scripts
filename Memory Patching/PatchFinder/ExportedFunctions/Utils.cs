using System;
using System.Linq;

namespace ExportedFunctions
{
    class Utils
    {
        public static bool parseArguments(string[] args)
        {
            getValues(args, out var inputFile, out var outputFile, out var functions, out var bytes, out var number);

            HookManager h = new HookManager(number);

            switch (args[0])
            {
                case "enumerate":
                    if(functions == null) 
                        h.enumExportedFunctions();
                    h.getFirstBytes(functions, outputFile);
                    break;
                case "check":
                        h.checkPatch(inputFile, functions, bytes);
                        break;
                case "patch":
                    h.unhook(inputFile, functions, bytes);
                    break;
                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
            return true;
        }

        public static void getValues(string[] args, out string inputFile, out string outputFile, out string functions, out string bytes, out int number)
        {
            inputFile = null;
            outputFile = null;
            functions = null;
            bytes = null;
            number = 13;
            var skip = args.Skip(1).ToArray();

            for(int i = 0; i < skip.Length - 1; i+=2)
            {
                switch (skip[i])
                {
                    case "-i":
                        inputFile = skip[i + 1];
                        break;
                    case "-o":
                        outputFile = skip[i + 1];
                        break;
                    case "-f":
                        functions = skip[i + 1];
                        break;
                    case "-b":
                        bytes = skip[i + 1];
                        break;
                    case "-n":
                        number = int.Parse(skip[i + 1]);
                        break;
                    default:
                        Console.WriteLine("Unknown option " + skip[i]);
                        break;
                }
            }
        } 
    }
}
