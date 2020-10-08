using System;
using System.Collections.Generic;
using System.Text;
using VoiceOfAKingdomDiscord.Modules;
using Discord.WebSocket;
using System.Linq;

namespace VoiceOfAKingdomDiscord.Commands
{
    class StartCommand: Command
    {
        public StartCommand()
        {
            Name = "start";
            Abbreviations.Add(Name);
            Description = "Starts a new game.";
            Parameters = new Dictionary<string, string>();
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
                App.GameMgr.Games.Add(new Game(cmdHandler.Msg.Author.Id, cmdHandler));
            }
        }
    }
}
