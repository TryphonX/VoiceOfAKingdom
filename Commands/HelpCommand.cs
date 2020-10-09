using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
            Abbreviations.Add(Name);
            Description = "Displays help message for all commands or a specific command.";
            Parameters = new Dictionary<string, string>
            {
                { "command", "The command to display help for." }
            };
        }

        public override void Run(CommandHandler cmdHandler)
        {
            base.Run(cmdHandler);

            Embed embed;
            
            // check if there is an arg
            if (cmdHandler.Args.Count > 0)
            {
                SpecificCommand = cmdHandler.Args[0];
                
                // Iterate through the commands to find if the
                // first arg is an actual command name
                Command command = null;
                foreach (Command cmd in cmdHandler.Commands)
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
                    embed = PrepareHelpEmbedAll(cmdHandler);
                }
                // If there is a command, show the help for the command used
                else
                {
                    embed = PrepareHelpEmbedSpecific(command);
                }
            }
            else
            {
                embed = PrepareHelpEmbedAll(cmdHandler);
            }

            cmdHandler.Msg.Channel.SendMessageAsync(embed: embed);
        }

        /// <summary>
        /// Prepares the Embed for the help message when there is no specific command requested.
        /// </summary>
        /// <param name="commandHandler"></param>
        /// <returns>The embed it created; ready to send</returns>
        private Embed PrepareHelpEmbedAll(CommandHandler commandHandler)
        {
            EmbedBuilder embed = new CustomEmbed()

            StringBuilder valueBuilder;
            foreach (Command command in commandHandler.Commands)
            {
                #region Field Format
                /* 
                 * !help `<command>`
                 * Abbreviations: !someAbbrev1, !someAbbrev2
                 * Displays help message for all commands or a specific command.
                 * `command: The command to display help for.`
                 */
                #endregion

                #region Building the value
                valueBuilder = new StringBuilder(command.Description);

                if (command.Abbreviations.Count > 1)
                {
                    valueBuilder.Append($"\nAbbreviations: {Config.Prefix}{GetJoinedAbbreviations(command)}");
                }

                if (command.Parameters.Count > 0)
                {
                    foreach (string param in command.Parameters.Keys)
                    {
                        valueBuilder.Append($"\n**<{param}>**: _{command.Parameters[param]}_");
                    }
                }
                #endregion

                embed.AddField(new EmbedFieldBuilder()
                    .WithName(GetCommandWithSyntax(command))
                    .WithValue(valueBuilder.ToString()));
            }

            return embed.Build();
        }

        /// <summary>
        /// Prepares the Embed for the help message when there is a specific command
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private Embed PrepareHelpEmbedSpecific(Command command)
        {
            EmbedBuilder embed = new CustomEmbed()
                .WithColor(Color.DarkPurple)
                .WithTitle(GetCommandWithSyntax(command));


            StringBuilder descriptionBuilder = new StringBuilder(command.Description);
            if (command.Abbreviations.Count > 1)
            {
                descriptionBuilder.Append($"\nAbbreviations: {Config.Prefix}{GetJoinedAbbreviations(command)}");
            }

            embed.WithDescription(descriptionBuilder.ToString());

            foreach (string param in command.Parameters.Keys)
            {
                embed.AddField(new EmbedFieldBuilder()
                    .WithName($"<{param}>")
                    .WithValue(command.Parameters[param])
                    .WithIsInline(true));
            }

            return embed.Build();
        }

        /// <summary>
        /// Example: !help `&lt;command&gt;`
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private string GetCommandWithSyntax(Command command)
        {
            StringBuilder sb = new StringBuilder($"{Config.Prefix}{command.Name}");

            if (command.Parameters.Count > 0)
            {
                foreach (string param in command.Parameters.Keys)
                {
                    sb.Append($" `<{param}>`");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Joins the abbreviations with the prefix as a separator.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private string GetJoinedAbbreviations(Command command) =>
            // Take all abbreviations, make them into a string and add a comma and the prefix between them
            // Start from index 1 (since index 0 will always be the name of the command)
            // Max abbreviations to "join" will be one less than its count (since we skip index 0)
            string.Join($", {Config.Prefix}", command.Abbreviations.ToArray(), startIndex: 1, command.Abbreviations.Count - 1);
    }
}
