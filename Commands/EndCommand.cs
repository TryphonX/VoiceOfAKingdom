using System;
using System.Collections.Generic;
using System.Text;
using VoiceOfAKingdomDiscord.Modules;

namespace VoiceOfAKingdomDiscord.Commands
{
    class EndCommand: Command
    {
        public EndCommand()
        {
            Name = "end";
            Abbreviations.Add(Name);
            Description = "Ends your current game.";
            Parameters = new Dictionary<string, string>();
        }

        public override void Run(CommandHandler cmdHandler)
        {
            base.Run(cmdHandler);

            if (GameManager.TryGetGame(cmdHandler.Msg.Author.Id, out Game game))
            {
                cmdHandler.Msg.Channel.SendMessageAsync("You ended your game.");

                GameManager.EndGame(game);
            }
            else
            {
                cmdHandler.Msg.Channel.SendMessageAsync("You have no running game.");
            }
        }
    }
}
