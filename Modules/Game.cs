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
        public DateTime BirthDate { get; } 
        public short Age { get; set; }
        public KingdomStats KingdomStats { get; set; } = new KingdomStats();
        public PersonalStats PersonalStats { get; set; } = new PersonalStats();
        public Request CurrentRequest { get; set; }
        public Request.Source RequestSource { get; }
        public bool IsDead { get; set; } = false;
        public bool IsCaptured { get; set; } = false;
        public int MonthsCaptured { get; set; } = 0;

        public Game(CommandHandler cmdHandler, Request.Source requestSource)
        {
            try
            {
                PlayerID = cmdHandler.Msg.Author.Id;
                RequestSource = requestSource;

                BirthDate = GetRandomBirthDate(Date);

                GameManager.UpdateAge(this);

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

                    CurrentRequest = GameManager.GetRandomRequest(RequestSource);
                    antecedent.Result.SendMessageAsync(embed: GameManager.GetNewRequestEmbed(this))
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

        private static DateTime GetRandomBirthDate(DateTime date)
        {
            // remove 16-32 years
            date = date.AddYears(-CommonScript.Rng.Next(16, 32));

            // randomizing the month
            int min = -date.Month + 1;
            int max = 12 - date.Month;
            date = date.AddMonths(CommonScript.Rng.Next(min, max));

            // randomizing the day
            min = -date.Day + 1;
            max = CommonScript.MonthsWith31Days.Any(month => month == date.Month)
                ? 31 - date.Day
                : 30 - date.Day;

            if (date.Month == 2)
            {
                if (date.Year % 4 == 0)
                {
                    max = 29 - date.Day;
                }
                else
                {
                    max = 28 - date.Day;
                }
            }

            return date.AddDays(CommonScript.Rng.Next(min, max));
        }
    }
}
