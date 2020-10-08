using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class Game
    {
        public ulong PlayerID { get; set; }
        public ulong ChannelID { get; set; }
        public KingdomStatsClass KingdomStats { get; set; } = new KingdomStatsClass();
        public PersonalStatsClass PersonalStats { get; set; } = new PersonalStatsClass();

        public Game(ulong userID, CommandHandler commandHandler)
        {
            PlayerID = userID;

            ulong guildID = 0;
            foreach (var guild in App.Client.Guilds)
            {
                if (guild.Channels.Any(channel => channel.Id == commandHandler.Msg.Channel.Id))
                {
                    guildID = guild.Id;
                }
            }
            ChannelID = App.Client.GetGuild(guildID).CreateTextChannelAsync($"{commandHandler.Msg.Author.Username} Game").Result.Id;
        }

        public class KingdomStatsClass
        {
            public int Reputation { get; set; }
            public int Wealth { get; set; }
            public int Population { get; set; }
            public int Military { get; set; }

            public KingdomStatsClass()
            {
                Reputation = 50;
                Wealth = 50;
                Population = 50;
                Military = 50;
            }
        }
        public class PersonalStatsClass
        {
            public int Happiness { get; set; }
            public int Sanity { get; set; }
            public int Strength { get; set; }
            public int Charisma { get; set; }

            public PersonalStatsClass()
            {
                Happiness = new Random().Next(30, 60);
                Sanity = new Random().Next(30, 60);
                Strength = new Random().Next(30, 60);
                Charisma = new Random().Next(30, 60);
            }
        }
    }
}
