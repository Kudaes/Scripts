
using System;
using System.Threading;


namespace ExportedFunctions
{
    class Program
    {
        

        static void Main(string[] args)
        {

            if(args.Length == 0)
            {
                Console.WriteLine("Not enough arguments");
                return;
            }

            DInvoke.PE.PE_MANUAL_MAP moduleDetails = DInvoke.Map.MapModuleToMemory(@"c:\windows\system32\dbghelp.dll");
            DInvoke.PE.PE_MANUAL_MAP moduleDetails2 = DInvoke.Map.MapModuleToMemory(@"c:\windows\system32\kernel32.dll");

            Console.WriteLine("Enter username:");

            // Create a string variable and get user input from the keyboard and store it in the variable
            string userName = Console.ReadLine();


            Utils.parseArguments(args);

            Thread.Sleep(1000000);
        }
    }
}
