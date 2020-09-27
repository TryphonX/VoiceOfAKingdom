using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using VoiceOfAKingdomDiscord.Modules;

namespace VoiceOfAKingdomDiscord.Commands
{
    class HelpCommand: Command
    {
        public HelpCommand()
        {
            Name = "Help";
            Abbreviations = new List<string>()
            {
                Name
            };
            RequiredPermission = Permission.AnyonePermission;
            Description = "";
        }

        public override void Run(CommandHandler commandHandler)
        {
            base.Run(commandHandler);

            StringBuilder helpMessage = new StringBuilder();
            foreach (Command command in commandHandler.Commands)
            {
                helpMessage.AppendLine($"{Config.Prefix}{command.Name}\n`{command.Description}`");
            }

            commandHandler.Msg.Channel.SendMessageAsync(helpMessage.ToString());
        }

    }
}
