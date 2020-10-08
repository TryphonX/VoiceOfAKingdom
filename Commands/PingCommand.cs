using System;
using System.Collections.Generic;
using System.Text;
using VoiceOfAKingdomDiscord.Modules;

namespace VoiceOfAKingdomDiscord.Commands
{
    class PingCommand: Command
    {
        public PingCommand()
        {
            Name = "ping";
            Abbreviations.Add(Name);
            Description = "Check if the bot is responding.";
            Parameters = new Dictionary<string, string>();
        }

        public override void Run(CommandHandler cmdHandler)
        {
            base.Run(cmdHandler);

            cmdHandler.Msg.Channel.SendMessageAsync("pong!");
        }
    }
}
