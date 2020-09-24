using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace VoiceOfAKingdomDiscord.Scripts.Modules
{
    static class CommonScript
    {
        public const string Version = "0.1.0";
        public const string Author = "TryphonX";
        public const string Title = "Voice of a Kingdom";

        public static void Log(string msg, string location) =>
            Console.WriteLine($"{msg} @ {location}");

        public static void LogError(string msg, string location) =>
            Console.WriteLine($"**ERR {msg} @ {location}");

        public static void DebugLog(object msg, string location)
        {
            if (!Config.IsDebug)
                return;

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} Debug: {msg} @ {location}");
            Console.ResetColor();
        }
    }
}
