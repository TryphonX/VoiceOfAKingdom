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

            GameManager.Load(cmdHandler);
        }
    }
}
