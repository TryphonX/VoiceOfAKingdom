using System;
using System.Collections.Generic;
using System.Text;
using VoiceOfAKingdomDiscord.Scripts.Modules;

namespace VoiceOfAKingdomDiscord.Scripts.Commands
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
