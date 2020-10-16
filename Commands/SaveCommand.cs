using System;
using System.Collections.Generic;
using System.Text;
using VoiceOfAKingdomDiscord.Modules;

namespace VoiceOfAKingdomDiscord.Commands
{
    class SaveCommand: Command
    {
        public SaveCommand()
        {
            Name = "save";
            Abbreviations.Add(Name);
            Description = "Saves your current game.";
        }

        public override void Run(CommandHandler cmdHandler)
        {
            base.Run(cmdHandler);

            RunAsync(cmdHandler);
        }

        private async void RunAsync(CommandHandler cmdHandler)
        {
            if (GameManager.TryGetGame(cmdHandler.Msg.Author.Id, out Game game))
            {
                if (GameManager.Save(game))
                {
                    await cmdHandler.Msg.Channel.SendMessageAsync("Save successful.");
                }
                else
                {
                    await cmdHandler.Msg.Channel.SendMessageAsync("Save failed.");
                }
            }
            else
            {
                await cmdHandler.Msg.Channel.SendMessageAsync("You don't have a running game.");
            }
        }
    }
}
