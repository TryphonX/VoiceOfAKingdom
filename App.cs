using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Discord;
using System.Text;
using VoiceOfAKingdomDiscord.Modules;
using System.Diagnostics;

namespace VoiceOfAKingdomDiscord
{
    class App
    {
        public static DiscordSocketClient Client { get; private set; }
        public static GameManager GameMgr { get; } = new GameManager();

        static void Main(string[] args)
        {
            Console.Title = $"{CommonScript.Title} v{CommonScript.Version}";
            Config.ReloadConfig();
            SendStartingMessage();

            Console.CancelKeyPress += OnCancelKeyPress;

            if (!string.IsNullOrEmpty(Config.Token))
                new App().MainAsync().GetAwaiter().GetResult();
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            CommonScript.Log("Searching for running games");

            int count = 0;
            if (GameMgr.Games.Count > 0)
            {
                foreach (var game in GameMgr.Games)
                {
                    // TODO: Save before ending
                    count = GameManager.EndGame(game) ? ++count : count;
                }
            }

            if (count > 0)
                CommonScript.Log($"{count} games ended");

            CommonScript.Log("Exiting");

            // Thanks you, Visual Studio,
            // Really cool.
            // (Had to add this due to a VS Debugger bug)
            if (Debugger.IsAttached)
                Environment.Exit(1);
        }

        public async Task MainAsync()
        {
            Client = new DiscordSocketClient();

            DiscordEventHandler.SetEventTasks();

            await Client.LoginAsync(TokenType.Bot, Config.Token);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private static void SendStartingMessage()
        {
            StringBuilder thickSeparatorLine = new StringBuilder();
            StringBuilder thinSeparatorLine = new StringBuilder();

            for (int i = 0; i < CommonScript.Title.Length; i++)
            {
                thickSeparatorLine.Append("=");
                thinSeparatorLine.Append("-");
            }

            #region Init Message Format
            /*
             * ==================
             * Voice of a Kingdom
             * ==================
             * By TryphonX
             * Version: {version}
             * ------------------
             * Collaborators:
             * {names}
             * ==================
             * DEBUG MODE
             * ==================
             */
            #endregion

            #region Printing Init Message
            Console.WriteLine(thickSeparatorLine);

            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(CommonScript.Title);

            Console.ResetColor();
            Console.WriteLine(thickSeparatorLine);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Version: {CommonScript.Version}\nBy {CommonScript.Author}");

            Console.ResetColor();
            Console.WriteLine(thinSeparatorLine);

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            StringBuilder collabSb = new StringBuilder("Collaborators: ");
            collabSb.AppendLine();
            foreach (string name in CommonScript.Collaborators)
            {
                collabSb.AppendLine(name);
            }
            Console.WriteLine(collabSb.ToString().Trim());

            Console.ResetColor();
            Console.WriteLine(thickSeparatorLine);

            if (Config.IsDebug)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DEBUG MODE");
                Console.ResetColor();
                Console.WriteLine(thickSeparatorLine);
            }
            #endregion
        }
    }
}
