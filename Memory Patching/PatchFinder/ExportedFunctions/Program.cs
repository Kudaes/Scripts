
using System;

namespace PatchFinder
{
    class Program
    {
        

        static void Main(string[] args)
        {

            if(args.Length == 0)
            {
                Console.WriteLine("Not enough arguments");
                Utils.printHelp();
                return;
            }

            Utils.parseArguments(args);

        }
    }
}
