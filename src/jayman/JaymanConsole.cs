using Microsoft.ClearScript.V8;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jayman
{


    public class JaymanConsole : IJaymanRunnerListener
    {

        private int lastCursorTop = 0;
        private string lastScript = "";

        private int Column1Width = 55;
        private int Column2Width = 8;
        private int Column3Width = 15;
        private int Column4Width = 8;

        public void WaitInput()
        {

            Console.ReadKey();
        }

        public void Start()
        {


            

        }

        public void OnProgressChanged(string script, string currentstatus, bool fail, string message, string time)
        {
            if (lastScript != script)
            {
                lastScript = script;
                Console.SetCursorPosition(0, lastCursorTop);
            }
            else
            {
                Console.SetCursorPosition(0, lastCursorTop - 1);
                Console.WriteLine($"");
                Console.SetCursorPosition(0, lastCursorTop - 1);
            }

            Console.Write($"|{script.PadRight(Column1Width)}");
            Console.Write($"|{time.PadRight(Column2Width)}");

            if (fail)
            {

                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;

                Console.Write($"|{currentstatus.Cut(Column3Width).PadRight(Column3Width)}");

                Console.ResetColor();
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.White;

                Console.Write($"|{currentstatus.Cut(Column3Width).PadRight(Column3Width)}");

                Console.ResetColor();
            }
            Console.Write($"|{message.PadRight(Column4Width)}");
            Console.WriteLine("");

            lastCursorTop = Console.CursorTop;

        }

        public void OnUpdateSummary(JaymanExecutionSummary summary)
        {
            int labelWidth = 20;
            Console.Write($"|{"_".Repeat(Column1Width).PadLeft(Column1Width)}");
            Console.Write($"|{"_".Repeat(Column2Width).PadLeft(Column2Width)}");
            Console.Write($"|{"_".Repeat(Column3Width).PadLeft(Column3Width)}");
            Console.Write($"|{"_".Repeat(Column4Width).PadLeft(Column4Width)}");
            Console.WriteLine("");
            Console.WriteLine($"{"|+ Total Requests".PadRight(labelWidth)}:{summary.TotalRequests.ToString().PadRight(Column1Width- labelWidth)}|");
            Console.WriteLine($"{"|+ Total Succedded".PadRight(labelWidth)}:{ $"{summary.TotalSuccedded}/{summary.TotalRequests}".PadRight(Column1Width - labelWidth) }|");
            Console.WriteLine($"{"|+ Total Failed ".PadRight(labelWidth)}:{$"{summary.TotalFailed}/{summary.TotalRequests}".PadRight(Column1Width - labelWidth)}|");
            Console.WriteLine($"|{"_".Repeat(Column1Width)}|");

            lastCursorTop = Console.CursorTop;
        }

        public void CollectionStarted(string name)
        {
            lastScript = "";
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"|{name.PadRight(Column1Width)}");
            Console.ResetColor();
            Console.WriteLine("");
            Console.Write($"|{"Script Name".PadRight(Column1Width)}");
            Console.Write($"|{"Time".PadRight(Column2Width)}");
            Console.Write($"|{"Status".PadRight(Column3Width)}");
            Console.Write($"|{"Message".PadRight(Column4Width)}");

            Console.WriteLine("");

            Console.Write($"|{"-".Repeat(Column1Width).PadLeft(Column1Width)}");
            Console.Write($"|{"-".Repeat(Column2Width).PadLeft(Column2Width)}");
            Console.Write($"|{"-".Repeat(Column3Width).PadLeft(Column3Width)}");
            Console.Write($"|{"-".Repeat(Column4Width).PadLeft(Column4Width)}");

            Console.WriteLine("");
            lastCursorTop = Console.CursorTop;
        }

        public void CollectionFinished()
        {
            Console.WriteLine("");

            lastCursorTop = Console.CursorTop;
        }
    }


    public static class PostmanConsoleShellExtension
    {
        public static string Repeat(this string e, int repeat)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < repeat; i++)
            {
                builder.Append(e);
            }
            return builder.ToString();
        }

        public static string Cut(this string e, int maxChars)
        {
            if (e.Length <= maxChars)
                return e;
            return e.Substring(0, maxChars - 3) + "...";
        }

    }



}
