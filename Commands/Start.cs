using System;
using System.Collections.Generic;
using System.Text;
using VoiceOfAKingdomDiscord.Modules;
using Discord.WebSocket;
using System.Linq;

namespace VoiceOfAKingdomDiscord.Commands
{
    class Start: Command
    {
        public Start()
        {
            Name = "start";
            Abbreviations.Add(Name);
            Description = "Start a new game.";
            Parameters = new Dictionary<string, string>();
        }

        public override void Run(CommandHandler commandHandler)
        {
            base.Run(commandHandler);

            if (GameManager.HasGame(App.GameMgr.Games, commandHandler.Msg.Author.Id))
            {
                commandHandler.Msg.Channel.SendMessageAsync("You already have an active game.");
            }
            else
            {
                App.GameMgr.Games.Add(new Game(commandHandler.Msg.Author.Id, commandHandler));

                commandHandler.Msg.Channel.SendMessageAsync("New game started. Look for your channel.");
            }
        }
    }
}
