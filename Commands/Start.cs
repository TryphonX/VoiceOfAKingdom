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

        public override void Run(CommandHandler cmdHandler)
        {
            base.Run(cmdHandler);

            if (GameManager.HasGame(App.GameMgr.Games, cmdHandler.Msg.Author.Id))
            {
                cmdHandler.Msg.Channel.SendMessageAsync("You already have an active game.");
            }
            else
            {
                App.GameMgr.Games.Add(new Game(cmdHandler.Msg.Author.Id, cmdHandler));

                cmdHandler.Msg.Channel.SendMessageAsync("New game started. Look for your channel.");
            }
        }
    }
}
