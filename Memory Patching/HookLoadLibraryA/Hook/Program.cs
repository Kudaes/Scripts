using System;
using System.Runtime.InteropServices;


namespace Hook
{
    class Program
    {

        static void Main(string[] args)
        {

            HookManager g = new HookManager();
            g.Install();

            while (true) {
                Console.Write("[*] Enter a dll name/path:");
                string dll = Console.ReadLine();
                g.load(dll);
            }

        }
    }
}
