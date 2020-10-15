using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VoiceOfAKingdomDiscord.Modules
{
    static class DiscordEventHandler
    {
        public static void SetEventTasks()
        {
            App.Client.Log += OnLog;
            App.Client.MessageReceived += OnMessageReceived;
            App.Client.ReactionAdded += CheckForGameReaction;
            App.Client.ReactionAdded += CheckForReload;
            App.Client.LatencyUpdated += OnLatencyUpdated;
            App.Client.Disconnected += OnDisconnected;
        }

        /// <summary>
        /// Checks if the reaction is a reply to the reload command.
        /// </summary>
        /// <param name="unCachedMsg"></param>
        /// <param name="channel"></param>
        /// <param name="reaction"></param>
        /// <returns></returns>
        private static Task CheckForReload(Cacheable<IUserMessage, ulong> unCachedMsg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId == App.Client.CurrentUser.Id)
                return Task.CompletedTask;

            if (reaction.UserId != Config.OwnerID)
                return Task.CompletedTask;

            // Reload msg check
            try
            {
                bool accepted = reaction.Emote.Name.Equals(CommonScript.UnicodeAccept);
                bool rejected = reaction.Emote.Name.Equals(CommonScript.UnicodeReject);

                if (accepted || rejected)
                {
                    channel.GetMessageAsync(reaction.MessageId)
                    .ContinueWith(antecedent =>
                    {
                        if (antecedent.Result != null &&
                        antecedent.Result.Author.Id == App.Client.CurrentUser.Id &&
                        antecedent.Result.Embeds.Any(embed => Regex.IsMatch(embed.Title, @"Are you sure you want to reload all custom requests\?")))
                        {
                            antecedent.Result.RemoveAllReactionsAsync();
                            if (accepted)
                            {
                                bool success = GameManager.ReloadRequests(true);
                                EmbedBuilder embed = new CustomEmbed()
                                    .WithColor(success ? Color.Green : Color.DarkRed)
                                    .WithTitle(success ? "Reload successful." : "Reload failed.");

                                if (!success)
                                {
                                    embed.WithDescription("Check your console.");
                                }

                                antecedent.Result.Channel.SendMessageAsync(embed: embed.Build()).Wait();
                            }
                            else
                            {
                                antecedent.Result.Channel.SendMessageAsync(embed: new CustomEmbed()
                                    .WithColor(Color.Blue)
                                    .WithTitle("Reload aborted.")
                                    .Build()).Wait();
                            }

                            antecedent.Result.DeleteAsync();
                        }
                    });

                    return Task.CompletedTask;
                }

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                CommonScript.LogError(e.Message);
                return Task.CompletedTask;
            }
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
                CommonScript.LogWarn($"High latency noted. Latency: {currentLatency}");
            }
            else if (currentLatency < 400 && previousLatency >= 400)
            {
                CommonScript.LogWarn($"Latency restored. Latency: {currentLatency}");
            }

            return Task.CompletedTask;
        }

        private static Task CheckForGameReaction(Cacheable<IUserMessage, ulong> unCachedMsg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId == App.Client.CurrentUser.Id)
                return Task.CompletedTask;

            if (GameManager.Games.Count == 0)
                return Task.CompletedTask;

            if (!GameManager.Games.Any(game => game.PlayerID == reaction.UserId))
                return Task.CompletedTask;

            try
            {
                // Check if the user has a game
                // if they do, proceed
                if (GameManager.TryGetGame(reaction.UserId, out Game game))
                {
                    // Not the game's channel
                    // Safe to exit the task
                    if (channel.Id != game.ChannelID)
                        return Task.CompletedTask;

                    bool accepted = reaction.Emote.Name.Equals(CommonScript.UnicodeAccept);
                    bool invalidReaction = !accepted && !reaction.Emote.Name.Equals(CommonScript.UnicodeReject);

                    if (!invalidReaction)
                    {
                        // Accepted or Rejected
                        if (!game.IsDead)
                        {
                            if (!accepted && game.IsCaptured)
                            {
                                // Somehow reacted with a no to a jailed message.
                                CommonScript.LogWarn($"Invalid reaction ({CommonScript.UnicodeReject}) for a finished game. Possibly wrong permissions.");
                            }

                            InitResolveRequestAsync(channel, reaction, game, accepted);
                            return Task.CompletedTask;
                        }
                        else
                        {
                            if (!accepted)
                            {
                                // Somehow reacted with a no to an end game message.
                                CommonScript.LogWarn($"Invalid reaction ({CommonScript.UnicodeReject}) for a finished game. Possibly wrong permissions.");
                            }

                            GameManager.EndGame(game);
                            return Task.CompletedTask;
                        }
                    }
                    else
                    {
                        // Unexpected behavior
                        // Someone most likely broke the permissions
                        CommonScript.LogWarn("Invalid reaction. Possibly wrong permissions.");
                        return Task.CompletedTask;
                    }
                }

                // No game
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                CommonScript.LogError(e.Message);
                return Task.CompletedTask;
            }
        }

        private static async void InitResolveRequestAsync(ISocketMessageChannel channel, SocketReaction reaction, Game game, bool accepted)
        {
            await channel.GetMessageAsync(reaction.MessageId)
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
