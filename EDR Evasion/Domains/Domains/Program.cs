using System;

namespace Domains
{
    class Program
    {
        static void Main(string[] args)
        {             
            while (!Utils.parseArguments(args)) {
                Console.Write("Select an option > ");
                args = Console.ReadLine().Split(' ');   
            }
        }
    }
    
}
