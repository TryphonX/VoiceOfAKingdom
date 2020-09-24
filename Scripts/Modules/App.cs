using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Discord;
using VoiceOfAKingdomDiscord.Scripts;
using System.Text;

namespace VoiceOfAKingdomDiscord
{
    class App
    {
        public static DiscordSocketClient Client { get; private set; }

        static void Main(string[] args)
        {
            Config.ReloadConfig();
            SendStartingMessage();
            if (!string.IsNullOrEmpty(Config.Token))
                new App().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            Client = new DiscordSocketClient();

            DiscordEventHandler.SetEventTasks(Client);

            await Client.LoginAsync(TokenType.Bot, Config.Token);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private static void SendStartingMessage()
        {
            StringBuilder separatorLine = new StringBuilder();

            for (int i = 0; i < CommonScript.Title.Length; i++)
            {
                separatorLine.Append("=");
            }

            #region Init Message Format
            /*
             * ==================
             * Voice of a Kingdom
             * ==================
             * By TryphonX
             * Version: {version}
             */
            #endregion

            #region Printing Init Message
            Console.WriteLine(separatorLine);

            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(CommonScript.Title);

            Console.ResetColor();
            Console.WriteLine(separatorLine);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Version: {CommonScript.Version}\nBy {CommonScript.Author}");

            Console.ResetColor();
            Console.WriteLine(separatorLine);

            if (Config.IsDebug)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("DEBUG MODE");
                Console.ResetColor();
                Console.WriteLine(separatorLine);
            }
            #endregion
        }
    }
}
