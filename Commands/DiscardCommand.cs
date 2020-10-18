using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VoiceOfAKingdomDiscord.Modules;

namespace VoiceOfAKingdomDiscord.Commands
{
    class DiscardCommand: Command
    {
        public DiscardCommand()
        {
            Name = "discard";
            Abbreviations.Add(Name);
            Description = "Discards your saved game.";
        }

        public override void Run(CommandHandler cmdHandler)
        {
            base.Run(cmdHandler);

            if (GameManager.DiscardSave(cmdHandler.Msg.Author.Id))
            {
                cmdHandler.Msg.Channel.SendMessageAsync("Save discarded.");
            }
            else
            {
                cmdHandler.Msg.Channel.SendMessageAsync("You have no save.");
            }
        }
    }
}
