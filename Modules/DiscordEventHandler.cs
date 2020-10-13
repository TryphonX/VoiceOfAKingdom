using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceOfAKingdomDiscord.Modules
{
    static class DiscordEventHandler
    {
        public static void SetEventTasks()
        {
            App.Client.Log += OnLog;
            App.Client.MessageReceived += OnMessageReceived;
            App.Client.ReactionAdded += OnReactionAdded;
            App.Client.LatencyUpdated += OnLatencyUpdated;
            App.Client.Disconnected += OnDisconnected;
        }

        private static Task OnDisconnected(Exception e)
        {
            CommonScript.LogError($"Disconnected. Exception: {e.Message}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Notifies the user about high latency or when it's restored.
        /// </summary>
        /// <param name="previousLatency"></param>
        /// <param name="currentLatency"></param>
        /// <returns></returns>
        private static Task OnLatencyUpdated(int previousLatency, int currentLatency)
        {
            if (currentLatency >= 400 && previousLatency < 400)
            {
                CommonScript.LogWarn($"High latency noted.\tLatency: {currentLatency}");
            }
            else if (currentLatency < 400 && previousLatency >= 400)
            {
                CommonScript.LogWarn($"Latency restored.\tLatency: {currentLatency}");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unCachedMessage"></param>
        /// <param name="channel"></param>
        /// <param name="reaction"></param>
        /// <returns></returns>
        private static Task OnReactionAdded(Cacheable<IUserMessage, ulong> unCachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (GameManager.Games.Count == 0 || reaction.UserId == App.Client.CurrentUser.Id)
                return Task.CompletedTask;

            if (!GameManager.Games.Any(game => game.PlayerID == reaction.UserId))
                return Task.CompletedTask;
            try
            {
                foreach (var game in GameManager.Games)
                {
                    // Not the game's player
                    if (reaction.UserId != game.PlayerID)
                        continue;

                    // Not the game's channel
                    // Safe to exit the task
                    if (channel.Id != game.ChannelID)
                        break;

                    if (reaction.Emote.Name.Equals(CommonScript.UnicodeAccept))
                    {
                        // Proceed to next month calculations
                        // Or end the game
                        if (!game.IsDead)
                        {
                            InitNewMonthPreparations(channel, reaction, game, true);
                        }
                        else
                        {
                            GameManager.EndGame(game);
                        }
                        break;
                    }
                    else if (reaction.Emote.Name.Equals(CommonScript.UnicodeReject))
                    {
                        // Proceed to next month calculations
                        if (!game.IsDead)
                        {
                            InitNewMonthPreparations(channel, reaction, game, false);
                        }
                        else
                        {
                            CommonScript.LogWarn("Invalid reaction for a finished game. Possibly wrong permissions.");
                            GameManager.EndGame(game);
                        }
                        break;
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
            catch (Exception e)
            {
                CommonScript.LogError(e.Message);
            }

            return Task.CompletedTask;
        }

        private static void InitNewMonthPreparations(ISocketMessageChannel channel, SocketReaction reaction, Game game, bool accepted)
        {
            channel.GetMessageAsync(reaction.MessageId)
                .ContinueWith(antecedent =>
                {
                    antecedent.Result.RemoveAllReactionsAsync();
                    if (antecedent.Result.Author.Id != App.Client.CurrentUser.Id)
                    {
                        CommonScript.LogWarn("Someone other than the bot sent a message. Wrong permissions.");
                    }
                });

            GameManager.ResolveRequest(game, accepted);
        }

        private static Task OnMessageReceived(SocketMessage msg)
        {
            if (!msg.Content.StartsWith(Config.Prefix) || msg.Author.IsBot)
                return Task.CompletedTask;

            new CommandHandler().Run(msg);

            return Task.CompletedTask;
        }

        private static Task OnLog(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
