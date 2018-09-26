/////////////XTREME HACK////////////////
///////////unknowncheats.me/////////////

using System;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;

namespace External_ESP_Base
{
    class Program
    {
        static string m_line = "External ESP Base by XTreme2010";
        static string m_credits = "Credits: IChooseYou (SDK), smallC";

        [STAThread]
        static void Main(string[] args)
        {
            Init();

            ConsoleSpiner spin = new ConsoleSpiner();

            while (true)
            {
                Process process;
                if (GetProcessesByName("bf1", out process))
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    ClearCurrentConsoleLine();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(new string('-', Console.WindowWidth - 1));
                    Console.WriteLine("Status: Loaded{0}", new string(' ', 15));
                    Console.WriteLine("Id: {0}", process.Id);

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(true);
                    Application.Run(new Overlay(process));
                    break;
                }
                spin.Turn();
                Thread.Sleep(100);
            }

            Console.ReadKey();
        }

        private static void Init()
        {
            Console.Title = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(m_line);
            Console.WriteLine(m_credits);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.Write("Wаiting fоr BFH...");
        }

        public static bool GetProcessesByName(string pName, out Process process)
        {
            Process[] pList = Process.GetProcessesByName(pName);
            process = pList.Length > 0 ? pList[0] : null;
            return process != null;
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);

            for (int i = 0; i < Console.WindowWidth; i++)
                Console.Write("");

            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
