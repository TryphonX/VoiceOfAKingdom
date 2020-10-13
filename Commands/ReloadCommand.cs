using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using VoiceOfAKingdomDiscord.Modules;

namespace VoiceOfAKingdomDiscord.Commands
{
    class ReloadCommand: Command
    {
        public ReloadCommand()
        {
            Name = "reload";
            Abbreviations.Add(Name);
            Description = "Reloads the custom requests document.";
            RequiredPermission = Permission.OwnerPermission;
        }

        public override void Run(CommandHandler cmdHandler)
        {
            base.Run(cmdHandler);

            cmdHandler.Msg.Channel.SendMessageAsync($"Are you sure you want to reload all custom requests?\n" +
                $"**This will likely affect the {GameManager.Games.Count} game(s) currently running.**")
                .ContinueWith(antecedent =>
                {
                    antecedent.Result.AddReactionAsync(new Emoji(CommonScript.UnicodeAccept));
                });
        }
    }
}
