using System;
using System.Collections.Generic;
using System.Text;
using VoiceOfAKingdomDiscord.Modules;
using Discord.WebSocket;
using System.Linq;
using System.Text.RegularExpressions;

namespace VoiceOfAKingdomDiscord.Commands
{
    class StartCommand: Command
    {
        private Request.Source requestSource = Request.Source.Default;
        public StartCommand()
        {
            Name = "start";
            Abbreviations.Add(Name);
            Abbreviations.Add("play");
            Description = "Starts a new game.";
            Parameters = new Dictionary<string, string>()
            {
                {"requests source", "The source of your game's requests. (optional)\n-c(ustom): custom.\n-m(ixed): default and custom."},
            };
        }

        public override void Run(CommandHandler cmdHandler)
        {
            base.Run(cmdHandler);

            if (GameManager.TryGetGame(cmdHandler.Msg.Author.Id, out Game game))
            {
                cmdHandler.Msg.Channel.SendMessageAsync($"You already have an active game \\➡️ <#{game.ChannelID}>.");
            }
            else
            {
                // Start a game

                // Find the correct request source
                if (Regex.IsMatch(cmdHandler.Args[0], @"-c(ustom)?"))
                {
                    if (GameManager.HasCustomRequests)
                    {
                        requestSource = Request.Source.Custom;
                    }
                    else
                    {
                        cmdHandler.Msg.Channel.SendMessageAsync("Could not load any custom requests. Starting the game with default requests instead.");
                    }
                }
                else if (Regex.IsMatch(cmdHandler.Args[0], @"-m(ixed)?"))
                {
                    if (GameManager.HasCustomRequests)
                    {
                        requestSource = Request.Source.Mixed;
                    }
                    else
                    {
                        cmdHandler.Msg.Channel.SendMessageAsync("Could not load any custom requests. Starting the game with default requests instead.");
                    }
                }

                GameManager.Games.Add(new Game(cmdHandler, requestSource));
            }
        }
    }
}
