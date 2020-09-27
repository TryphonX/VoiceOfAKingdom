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
            Abbreviations = new List<string>()
            {
                Name
            };
            RequiredPermission = Permission.AnyonePermission;
            Description = "Check if the bot is responding.";
            Parameters = new Dictionary<string, string>();
        }

        public override void Run(CommandHandler commandHandler)
        {
            base.Run(commandHandler);

            commandHandler.Msg.Channel.SendMessageAsync("pong");
        }
    }
}
