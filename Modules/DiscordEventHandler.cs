using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace VoiceOfAKingdomDiscord.Modules
{
    static class DiscordEventHandler
    {
        public static void SetEventTasks()
        {
            App.Client.Log += Client_Log;
            App.Client.MessageReceived += Client_MessageReceived;
            App.Client.ReactionAdded += Client_ReactionAdded;
        }

        private static Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel channel, SocketReaction reaction)
        {
            IUserMessage msg;
            
            arg1.GetOrDownloadAsync().ContinueWith(antecedent =>
            {
                msg = antecedent.Result;

                // Ignore any reaction by non-players
                if (msg.Author.Id == App.Client.CurrentUser.Id &&
                    App.GameMgr.Games.Any(game => game.PlayerID == reaction.UserId))
                {
                    foreach (var game in App.GameMgr.Games)
                    {
                        // Not the game's player
                        if (reaction.UserId != game.PlayerID)
                            continue;

                        // Not the game's channel
                        // Safe to exit the task
                        if (channel.Id != game.ChannelID)
                            break;

                        if (reaction.Emote.Name.Equals(CommonScript.CHECKMARK))
                        {
                            // Proceed to next month calculations
                            msg.RemoveAllReactionsAsync();
                            GameManager.ResolveRequest(game, true);
                        }
                        else if (reaction.Emote.Name.Equals(CommonScript.NO_ENTRY))
                        {
                            // Proceed to next month calculations
                            msg.RemoveAllReactionsAsync();
                            GameManager.ResolveRequest(game, false);
                        }
                        else
                        {
                            // Unexpected behavior
                            // Someone most likely broke the permissions
                            CommonScript.LogWarn("Invalid reaction. Possibly wrong permissions.");
                            break;
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }

        private static Task Client_MessageReceived(SocketMessage msg)
        {
            if (!msg.Content.StartsWith(Config.Prefix) || msg.Author.IsBot)
                return Task.CompletedTask;

            new CommandHandler().Run(msg);

            return Task.CompletedTask;
        }

        private static Task Client_Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
