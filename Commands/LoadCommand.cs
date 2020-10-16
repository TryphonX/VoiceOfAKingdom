using System;
using System.Collections.Generic;
using System.Text;
using VoiceOfAKingdomDiscord.Modules;

namespace VoiceOfAKingdomDiscord.Commands
{
    class LoadCommand: Command
    {
        public LoadCommand()
        {
            Name = "load";
            Abbreviations.Add(Name);
            Description = "Loads your last saved game.";
        }

        public override void Run(CommandHandler cmdHandler)
        {
            base.Run(cmdHandler);

            if (GameManager.HasGame(cmdHandler.Msg.Author.Id))
            {
                cmdHandler.Msg.Channel.SendMessageAsync("You already have a running game.");
            }

            GameManager.Load(cmdHandler);
        }
    }
}
