
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

            Utils.parseArguments(args);

        }
    }
}
