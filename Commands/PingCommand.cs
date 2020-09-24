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
            Name = "Ping";
            Abbreviations = new List<string>()
            {
                Name
            };
            RequiredPermission = Permission.AnyonePermission;
        }

        public override void Run(CommandHandler commandHandler)
        {
            base.Run(commandHandler);

            commandHandler.Msg.Channel.SendMessageAsync("pong");
        }
    }
}
