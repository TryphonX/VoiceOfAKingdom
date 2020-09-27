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
            Description = "Displays help message for all commands or a specific commands";
            Parameters = new Dictionary<string, string>
            {
                { "command", "The command to display help for." }
            };
        }

        public override void Run(CommandHandler commandHandler)
        {
            base.Run(commandHandler);

            StringBuilder helpMessage = new StringBuilder();
            
            // check if there is an arg
            if (commandHandler.Args.Count > 0)
            {
                SpecificCommand = commandHandler.Args[0];
                
                // Iterate through the commands to find if the
                // first arg is an actual command name
                Command command = null;
                foreach (Command cmd in commandHandler.Commands)
                {
                    // If any of the abbreviations is mentioned save the command
                    if (cmd.Abbreviations.Any(abbrev => abbrev.Equals(SpecificCommand, StringComparison.OrdinalIgnoreCase)))
                    {
                        command = cmd;
                    }
                }

                // If there was no command mentioned, show the help for all commands
                if (command == null)
                {
                    SendHelpAll(commandHandler, helpMessage);
                }
                // If there is a command, show the help for the command used
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
            // Example: !help
            helpMessage.Append($"**{Config.Prefix}{command.Name}**");

            // Example: !help <param> <param>
            if (command.Parameters.Count > 0)
            {
                foreach (string param in command.Parameters.Keys)
                {
                    helpMessage.Append($" `<{param}>`");
                }
            }

            // Add the description if any
            if (!string.IsNullOrEmpty(command.Description))
            {
                helpMessage.Append($"\n`{command.Description}`");
            }
            // Show patameter descriptions if any
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

        private Embed PrepareEmbed(Command command)
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.Color = Color.DarkPurple;
            eb.Title = command.Name;
            eb.Description = command.Description;
            return eb.Build();

        }
    }
}
