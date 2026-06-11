using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursorSchemeInverter
{
    public static class Logger
    {
        public static void HorizontalLine()
        {
            Console.WriteLine("========================================");
        }

        public static void BlankLine()
        {
            Console.WriteLine();
        }

        public static void Text(string message)
        {
            Console.WriteLine(message);
        }

        public static void Header(string title)
        {
            HorizontalLine();
            Console.WriteLine($"       {title}       ");
            HorizontalLine();
        }

        public static void Info(string message)
        {
            Console.WriteLine($"[Info] {message}");
        }

        public static void Success(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  [Converted] {message}");
            Console.ResetColor();
        }

        public static void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Warning] {message}");
            Console.ResetColor();
        }

        public static void Error(string context, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [Error - {context}] {message}");
            Console.ResetColor();
        }

        public static void Critical(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"\n[Critical Error] {message}");
            Console.ResetColor();
        }
    }
}
