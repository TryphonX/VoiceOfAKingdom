using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord.WebSocket;
using Discord;
using Discord.Rest;
using System.Xml;
using System.Globalization;

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

                GetGuildAndCategory(cmdHandler, out SocketGuild cachedGuild, out ulong categoryID);

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

        private void GetGuildAndCategory(CommandHandler cmdHandler, out SocketGuild cachedGuild, out ulong categoryID)
        {
            cachedGuild = null;
            categoryID = 0;

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

        public static Game Parse(XmlElement element, CommandHandler cmdHandler)
        {
            ulong playerID = cmdHandler.Msg.Author.Id;
            DateTime date = new DateTime();
            int monthsInControl = 0;
            DateTime birthDate = new DateTime();
            KingdomStats kingdomStats = null;
            PersonalStats personalStats = null;
            Request currentRequest = null;
            Request.Source requestSource = Request.Source.Default;
            bool isCaptured = false;
            int monthsCaptured = 0;

            foreach (XmlNode node in element)
            {
                if (node.Name.Equals("Date"))
                {
                    date = DateTime.Parse(node.InnerText, styles: DateTimeStyles.AssumeLocal);
                    continue;
                }
                if (node.Name.Equals("MonthsInControl"))
                {
                    monthsInControl = int.Parse(node.InnerText);
                    continue;
                }
                if (node.Name.Equals("BirthDate"))
                {
                    birthDate = DateTime.Parse(node.InnerText, styles: DateTimeStyles.AssumeLocal);
                    continue;
                }
                if (node.Name.Equals("KingdomStats"))
                {
                    kingdomStats = KingdomStats.Parse(node.InnerText);
                    continue;
                }
                if (node.Name.Equals("PersonalStats"))
                {
                    personalStats = PersonalStats.Parse(node.InnerText);
                    continue;
                }
                if (node.Name.Equals("CurrentRequest"))
                {
                    currentRequest = Request.Parse(node.InnerText);
                    continue;
                }
                if (node.Name.Equals("RequestSource"))
                {
                    requestSource = Enum.Parse<Request.Source>(node.InnerText);
                    continue;
                }
                if (node.Name.Equals("IsCaptured"))
                {
                    isCaptured = bool.Parse(node.InnerText);
                    continue;
                }
                if (node.Name.Equals("MonthsCaptured"))
                {
                    monthsCaptured = int.Parse(node.InnerText);
                    continue;
                }
            }

            return new Game(playerID, date, monthsInControl, birthDate, kingdomStats, personalStats,
                currentRequest, requestSource, isCaptured, monthsCaptured, cmdHandler);
        }

        private Game(ulong playerID, DateTime date, int monthsInControl,
            DateTime birthDate, KingdomStats kingdomStats, PersonalStats personalStats,
            Request request, Request.Source requestSource, bool isCaptured, int monthsCaptured,
            CommandHandler cmdHandler)
        {
            PlayerID = playerID;
            Date = date;
            MonthsInControl = monthsInControl;
            BirthDate = birthDate;
            KingdomStats = kingdomStats;
            PersonalStats = personalStats;

            if (requestSource != Request.Source.Default && !GameManager.HasCustomRequests)
                RequestSource = Request.Source.Default;
            else
                RequestSource = requestSource;

            if (request == null)
                CurrentRequest = GameManager.GetRandomRequest(RequestSource);
            else
                CurrentRequest = request;

            IsCaptured = isCaptured;
            MonthsCaptured = monthsCaptured;

            GameManager.UpdateAge(this);
            IsDead = false;

            try
            {
                GetGuildAndCategory(cmdHandler, out SocketGuild cachedGuild, out ulong categoryID);

                cachedGuild.CreateTextChannelAsync($"{cmdHandler.Msg.Author.Username} Game", channel =>
                {
                    channel.CategoryId = categoryID;
                }).ContinueWith(antecedent =>
                {
                    ChannelID = antecedent.Result.Id;

                    // Give the player the permission to send messages
                    antecedent.Result.AddPermissionOverwriteAsync(App.Client.GetGuild(cachedGuild.Id).GetUser(PlayerID),
                        new OverwritePermissions(sendMessages: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow));

                    antecedent.Result.SendMessageAsync(embed: GameManager.GetNewRequestEmbed(this))
                        .ContinueWith(antecedent =>
                        {
                            if (KingdomStats.Wealth + CurrentRequest.KingdomStatsOnAccept.Wealth >= 0)
                                antecedent.Result.AddReactionAsync(new Emoji(CommonScript.UnicodeAccept)).Wait();
                            if (KingdomStats.Wealth + CurrentRequest.KingdomStatsOnReject.Wealth >= 0)
                                antecedent.Result.AddReactionAsync(new Emoji(CommonScript.UnicodeReject)).Wait();

                        });

                    cmdHandler.Msg.Channel.SendMessageAsync($"Game loaded \\➡️ <#{antecedent.Result.Id}>");
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
