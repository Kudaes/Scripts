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
                case "-h":
                    printHelp();
                    break;
                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
            return true;
        }

        public static void printHelp()
        {
            var help = @"
usage:  patchFinder.exe enumerate -o output.txt [options]
        patchFinder.exe check [options]
        patchFinder.exe patch [options]

  options:
    -i  Input file (usefull to check or patch). 
    -o  Save output to file (mandatory when using 'enumerate').  
    -b  Bytes value used to check for hooks or used to patch some function (e.g. -b '4010AA320204'). 
    -f  Concrete function used to enumerate/check/patch hooks (e.g. kernel32>LoadLibraryA&ntdll.dll>NtCreateFile).

";
            Console.WriteLine(help);
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
