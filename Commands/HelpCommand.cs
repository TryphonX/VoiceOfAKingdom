using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoiceOfAKingdomDiscord.Modules;

namespace VoiceOfAKingdomDiscord.Commands
{
    class HelpCommand: Command
    {
        private string SpecificCommand { get; set; }

        public HelpCommand()
        {
            Name = "help";
            Abbreviations = new List<string>()
            {
                Name
            };
            RequiredPermission = Permission.AnyonePermission;
            Description = "";
            Parameters = new Dictionary<string, string>
            {
                { "command", "The command you need help with." }
            };
        }

        public override void Run(CommandHandler commandHandler)
        {
            base.Run(commandHandler);

            StringBuilder helpMessage = new StringBuilder();
            if (commandHandler.Args.Count > 0)
            {
                SpecificCommand = commandHandler.Args[0];

                Command command = null;

                foreach (Command cmd in commandHandler.Commands)
                {
                    if (cmd.Abbreviations.Any(abbrev => abbrev.Equals(SpecificCommand, StringComparison.OrdinalIgnoreCase)))
                    {
                        command = cmd;
                    }
                }

                if (command == null)
                {
                    SendHelpAll(commandHandler, helpMessage);
                }
                else
                {
                    PrepareHelpMessage(command, helpMessage);
                }
            }
            else
            {
                SendHelpAll(commandHandler, helpMessage);
            }

            commandHandler.Msg.Channel.SendMessageAsync(helpMessage.ToString());
        }

        private void PrepareHelpMessage(Command command, StringBuilder helpMessage)
        {
            helpMessage.Append($"**{Config.Prefix}{command.Name}**");

            if (command.Parameters.Count > 0)
            {
                foreach (string param in command.Parameters.Keys)
                {
                    helpMessage.Append($" `<{param}>`");
                }
            }

            if (!string.IsNullOrEmpty(command.Description))
            {
                helpMessage.Append($"\n`{command.Description}`");
            }
            if (command.Parameters.Count > 0)
            {
                foreach (string param in command.Parameters.Keys)
                {
                    helpMessage.Append($"\n{param}: {command.Parameters[param]}");
                }
            }
        }

        private void SendHelpAll(CommandHandler commandHandler, StringBuilder helpMessage)
        {
            foreach (Command command in commandHandler.Commands)
            {
                PrepareHelpMessage(command, helpMessage);

                helpMessage.AppendLine();
            }
        }
    }
}
