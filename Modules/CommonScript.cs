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
        public static EmbedFooterBuilder EmbedFooter { get; } = new EmbedFooterBuilder()
            .WithText($"👑 Voice of a Kingdom v{Version}");

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

        public static void DebugLog(object msg)
        {
            if (!Config.IsDebug)
                return;

            StackFrame stackFrame = new StackTrace(1, true).GetFrame(0);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} Debug: {msg} @ {GetClassName(stackFrame.GetFileName())}.{stackFrame.GetMethod().Name}");
            Console.ResetColor();
        }

        private static string GetClassName(string fileName) =>
            fileName.Split('\\').Last().TrimEnd('s', 'c', '.');
    }
}
