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
        public ulong PlayerID { get; set; }
        public ulong ChannelID { get; set; }
        public ulong GuildID { get; set; }
        public ulong CategoryID { get; }
        public DateTime Date { get; set; } = CommonScript.GetRandomDate();
        public int MonthsInControl { get; set; } = 0;
        public KingdomStatsClass KingdomStats { get; set; } = new KingdomStatsClass();
        public PersonalStatsClass PersonalStats { get; set; } = new PersonalStatsClass();
        public Request CurrentRequest { get; set; }
        public bool IsDead { get; set; } = false;

        public Game(ulong userID, CommandHandler cmdHandler)
        {
            try
            {
                PlayerID = userID;

                SocketGuild cachedGuild = null;

                GuildID = 0;
                CategoryID = 0;
                foreach (var guild in App.Client.Guilds)
                {
                    if (guild.Channels.Any(channel => channel.Id == cmdHandler.Msg.Channel.Id))
                    {
                        GuildID = guild.Id;
                        cachedGuild = guild;

                        // Last Voice of a Kingdom category
                        SocketCategoryChannel category = guild.CategoryChannels
                            .Last(category => category.Name.Contains("Voice of a Kingdom", StringComparison.OrdinalIgnoreCase));

                        if (category == null)
                        {
                            cmdHandler.Msg.Channel.SendMessageAsync("There is no \"Voice of a Kingdom\" category in the server.");
                            throw new Exception("Missing category");
                        }

                        CategoryID = category.Id;
                    }
                }

                if (cachedGuild == null)
                    throw new Exception();

                if (CategoryID == 0)
                {
                    cmdHandler.Msg.Channel.SendMessageAsync("There is no \"Voice of a Kingdom\" category in the server.");
                    throw new Exception("Missing category");
                }

                cachedGuild.CreateTextChannelAsync($"{cmdHandler.Msg.Author.Username} Game", channel =>
                {
                    channel.CategoryId = CategoryID;
                }).ContinueWith(antecedent =>
                {
                    ChannelID = antecedent.Result.Id;

                    // Give the player the permission to send messages
                    antecedent.Result.AddPermissionOverwriteAsync(App.Client.GetGuild(GuildID).GetUser(PlayerID),
                        new OverwritePermissions(sendMessages: PermValue.Allow, manageChannel: PermValue.Allow));

                    CurrentRequest = GameManager.GetRandomRequest();
                    antecedent.Result.SendMessageAsync(embed: GameManager.GetNewMonthEmbed(this, true))
                        .ContinueWith(antecedent =>
                        {
                            antecedent.Result.AddReactionAsync(new Emoji(CommonScript.CHECKMARK)).Wait();
                            antecedent.Result.AddReactionAsync(new Emoji(CommonScript.NO_ENTRY)).Wait();
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

        public class KingdomStatsClass
        {
            public short Folks { get; set; }
            public short Military { get; set; }
            public short Nobles { get; set; }
            public short Wealth { get; set; }

            public KingdomStatsClass()
            {
                Random random = new Random();

                Folks = (short)random.Next(40, 60);
                Wealth = (short)random.Next(40, 60);
                Nobles = (short)random.Next(40, 60);
                Military = (short)random.Next(40, 60);
            }

            public KingdomStatsClass(short folks = 0, short nobles = 0, short military = 0, short wealth = 0)
            {
                Folks = folks;
                Nobles = nobles;
                Military = military;
                Wealth = wealth;
            }

            public static KingdomStatsClass operator +(KingdomStatsClass kingdomStats, KingdomStatsClass incKingdomStats)
            {
                kingdomStats.Folks += incKingdomStats.Folks;
                kingdomStats.Folks = CommonScript.Check0To100Range(kingdomStats.Folks);

                kingdomStats.Nobles += incKingdomStats.Nobles;
                kingdomStats.Nobles = CommonScript.Check0To100Range(kingdomStats.Nobles);

                kingdomStats.Wealth += incKingdomStats.Wealth;
                kingdomStats.Wealth = CommonScript.Check0To100Range(kingdomStats.Wealth);

                kingdomStats.Military += incKingdomStats.Military;
                kingdomStats.Military = CommonScript.Check0To100Range(kingdomStats.Military);

                return kingdomStats;
            }

            /// <summary>
            /// Well ALMOST invert them. Inverting them would be too harsh on the player.
            /// </summary>
            public void InvertReputations()
            {
                Folks = (short)Math.Abs(40 - Folks);
                Nobles = (short)Math.Abs(40 - Nobles);
                Military = (short)Math.Abs(40 - Military);
            }

            public void IncValues(short incFolks = 0, short incNobles = 0, short incMilitary = 0, short incWealth = 0)
            {
                Folks += incFolks;
                Nobles += incNobles;
                Military += incMilitary;
                Wealth += incWealth;
            }
        }
        public class PersonalStatsClass
        {
            public short Happiness { get; set; }
            public short Charisma { get; set; }

            public PersonalStatsClass()
            {
                Random random = new Random();

                Happiness = (short)random.Next(30, 60);
                Charisma = (short)random.Next(30, 60);
            }

            public PersonalStatsClass(short happiness = 0, short charisma = 0)
            {
                Happiness = happiness;
                Charisma = charisma;
            }

            public static PersonalStatsClass operator +(PersonalStatsClass personalStats, PersonalStatsClass incPersonalStats)
            {
                personalStats.Happiness += incPersonalStats.Happiness;
                personalStats.Happiness = CommonScript.Check0To100Range(personalStats.Happiness);

                personalStats.Charisma += incPersonalStats.Charisma;
                personalStats.Charisma = CommonScript.Check0To100Range(personalStats.Charisma);

                return personalStats;
            }

            public void IncValues(short incHappiness = 0, short incCharisma = 0)
            {
                Happiness += incHappiness;
                Charisma += incCharisma;
            }
        }
    }
}
