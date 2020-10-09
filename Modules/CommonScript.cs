using Discord;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    static class CommonScript
    {
        public static string Version { get; } = "0.1.0";
        public static string Author { get; } = "Tryphon Ksydas";
        public static string[] Collaborators { get; } = { "ZarOS69" };
        public static string Title { get; } = "Voice of a Kingdom";

        public static void Log(string msg)
        {
            StackFrame stackFrame = new StackTrace(1, true).GetFrame(0);

            Console.WriteLine($"{msg} @ {GetClassName(stackFrame.GetFileName())}.{stackFrame.GetMethod().Name}");
        }

        public static void LogError(string msg)
        {
            StackFrame errorFrame = new StackTrace(1, true).GetFrame(0);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"**ERR {msg} @ {GetClassName(errorFrame.GetFileName())}.{errorFrame.GetMethod().Name}");
            Console.ResetColor();
        }

        public static void DebugLog(object msg, bool skipOneFrame = false)
        {
            if (!Config.IsDebug)
                return;

            StackFrame stackFrame;
            if (skipOneFrame)
            {
                stackFrame = new StackTrace(2, true).GetFrame(0);
            }
            else
            {
                stackFrame = new StackTrace(1, true).GetFrame(0);
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} Debug: {msg} @ {GetClassName(stackFrame.GetFileName())}.{stackFrame.GetMethod().Name}");
            Console.ResetColor();
        }

        private static string GetClassName(string fileName) =>
            fileName.Split('\\').Last().TrimEnd('s', 'c', '.');

        public static DateTime GetRandomDate()
        {
            DateTime start = new DateTime(1600, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(new Random().Next(range));
        }

        public static int RoundToX(int num, int roundTo = 10)
        {
            int rem = num % roundTo;
            return rem >= roundTo/2 ? (num - rem + roundTo) : (num - rem);
        }

        public static EmbedFieldBuilder EmptyEmbedField()
        {
            return new EmbedFieldBuilder()
                .WithName("\u200B")
                .WithValue("\u200B");
        }
    }
}
