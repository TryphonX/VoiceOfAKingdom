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

        static void Main(string[] args)
        {
            Console.Title = $"{CommonScript.Title} v{CommonScript.Version}";
            Config.Reload();
            SendStartingMessage();

            GameManager.ReloadRequests();

            Console.CancelKeyPress += OnCancelKeyPress;

            if (!string.IsNullOrEmpty(Config.Token))
                new App().MainAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// No idea if this even works anymore due to VS debugger bugs
        /// The debugger decides to ignore the known bug where it doesn't exit
        /// and goes for another bug: exiting before the code is even over.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            CommonScript.Log("Ending running games");

            if (GameManager.Games.Count > 0)
            {
                foreach (var game in GameManager.Games)
                {
                    GameManager.Save(game);
                    GameManager.EndGame(game);
                }
            }

            CommonScript.Log("Exiting");

            // Thank you, Visual Studio,
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
