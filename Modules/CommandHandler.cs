using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VoiceOfAKingdomDiscord.Commands;

namespace VoiceOfAKingdomDiscord.Modules
{
    class CommandHandler
    {
        private string commandName;
        public List<string> Args { get; private set; }
        public SocketMessage Msg { get; private set; }
        public List<Command> Commands { get; set; }

        public void Run(SocketMessage msg)
        {
            Msg = msg;

            // Removed the prefix
            string strippedMsgContent = Regex.Replace(msg.Content, $"^{Config.Prefix}", "");
            
            // Get all the words in an array
            // used later for the arguments
            string[] msgWords = strippedMsgContent.Split(' ');

            commandName = msgWords[0];

            Args = new List<string>();
            for (int i = 1; i < msgWords.Length; i++)
            {
                Args.Add(msgWords[i]);
            }

            // Look for the command
            InitCommands();
            foreach (Command cmd in Commands)
            {
                if (cmd.Abbreviations.Any(abbrev => abbrev.Equals(commandName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (GetUserHasPermission(msg.Author, cmd))
                    {
                        cmd.Run(this);
                    }
                    else
                    {
                        msg.Channel.SendMessageAsync($"You need `{cmd.RequiredPermission.Name}` permission for this command.");
                    }
                    break;
                }
            }
        }

        private void InitCommands()
        {
            Commands = new List<Command>
            {
                new PingCommand(),
                new HelpCommand(),
                new StartCommand(),
                new EndCommand()
            };
        }

        private bool GetUserHasPermission(SocketUser user, Command cmd)
        {
            return Permission.GetUserPermissionPower(user) >= cmd.RequiredPermission.Power;
        }
    }
}
