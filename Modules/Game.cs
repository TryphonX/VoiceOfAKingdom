using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using Discord.Rest;

namespace VoiceOfAKingdomDiscord.Modules
{
    class Game
    {
        public ulong PlayerID { get; }
        public ulong ChannelID { get; private set; }
        public DateTime Date { get; set; } = CommonScript.GetRandomDate();
        public int MonthsInControl { get; set; } = 0;
        public KingdomStatsClass KingdomStats { get; set; } = new KingdomStatsClass();
        public PersonalStatsClass PersonalStats { get; set; } = new PersonalStatsClass();
        public Request CurrentRequest { get; set; }
        public Request.Source RequestSource { get; }
        public bool IsDead { get; set; } = false;

        public Game(CommandHandler cmdHandler, Request.Source requestSource)
        {
            try
            {
                PlayerID = cmdHandler.Msg.Author.Id;
                RequestSource = requestSource;

                SocketGuild cachedGuild = null;
                ulong categoryID = 0;

                foreach (var guild in App.Client.Guilds)
                {
                    if (guild.Channels.Any(channel => channel.Id == cmdHandler.Msg.Channel.Id))
                    {
                        cachedGuild = guild;

                        // Last Voice of a Kingdom category
                        SocketCategoryChannel category = guild.CategoryChannels
                            .Last(category => category.Name.Contains("Voice of a Kingdom", StringComparison.OrdinalIgnoreCase));

                        if (category == null)
                        {
                            cmdHandler.Msg.Channel.SendMessageAsync("There is no \"Voice of a Kingdom\" category in the server.");
                            throw new Exception("Missing category");
                        }

                        categoryID = category.Id;
                    }
                }

                if (cachedGuild == null)
                    throw new Exception();

                cachedGuild.CreateTextChannelAsync($"{cmdHandler.Msg.Author.Username} Game", channel =>
                {
                    channel.CategoryId = categoryID;
                }).ContinueWith(antecedent =>
                {
                    ChannelID = antecedent.Result.Id;

                    // Give the player the permission to send messages
                    antecedent.Result.AddPermissionOverwriteAsync(App.Client.GetGuild(cachedGuild.Id).GetUser(PlayerID),
                        new OverwritePermissions(sendMessages: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow));

                    CurrentRequest = GameManager.GetRandomRequest(this.RequestSource);
                    antecedent.Result.SendMessageAsync(embed: GameManager.GetNewMonthEmbed(this))
                        .ContinueWith(antecedent =>
                        {
                            antecedent.Result.AddReactionAsync(new Emoji(CommonScript.UnicodeAccept)).Wait();
                            antecedent.Result.AddReactionAsync(new Emoji(CommonScript.UnicodeReject)).Wait();
                        });

                    cmdHandler.Msg.Channel.SendMessageAsync($"New game started \\➡️ <#{antecedent.Result.Id}>");
                });
            }
            catch (Exception e)
            {
                CommonScript.LogError(e.Message);
                throw;
            }
        }
    }
}
