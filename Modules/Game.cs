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
        public DateTime Date { get; set; } = CommonScript.GetRandomDate();
        public int MonthsInControl { get; set; } = 0;
        public KingdomStatsClass KingdomStats { get; set; } = new KingdomStatsClass();
        public PersonalStatsClass PersonalStats { get; set; } = new PersonalStatsClass();

        public Game(ulong userID, CommandHandler commandHandler)
        {
            PlayerID = userID;

            GuildID = 0;
            foreach (var guild in App.Client.Guilds)
            {
                if (guild.Channels.Any(channel => channel.Id == commandHandler.Msg.Channel.Id))
                {
                    GuildID = guild.Id;
                }
            }

            App.Client.GetGuild(GuildID).CreateTextChannelAsync($"{commandHandler.Msg.Author.Username} Game", channel =>
            {
                channel.CategoryId = Config.GamesCategoryID;
            }).ContinueWith(antecedent =>
            {
                ChannelID = antecedent.Result.Id;

                // Give the player the permission to send messages
                antecedent.Result.AddPermissionOverwriteAsync(App.Client.GetGuild(GuildID).GetUser(PlayerID),
                    new OverwritePermissions(sendMessages: PermValue.Allow, manageChannel: PermValue.Allow));

                ISocketMessageChannel channel = (ISocketMessageChannel)GameManager.GetGameChannel(this);
                channel.SendMessageAsync(embed: App.GameMgr.GetNewMonthEmbed(this));

                commandHandler.Msg.Channel.SendMessageAsync($"New game started \\➡️ <#{antecedent.Result.Id}>");
            });
        }

        public class KingdomStatsClass
        {
            public int Folks { get; set; }
            public int Military { get; set; }
            public int Nobles { get; set; }
            public int Wealth { get; set; }

            public KingdomStatsClass()
            {
                Random random = new Random();

                Folks = random.Next(40, 60);
                Wealth = random.Next(40, 60);
                Nobles = random.Next(40, 60);
                Military = random.Next(40, 60);
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
                Random random = new Random();

                Happiness = random.Next(30, 60);
                Sanity = random.Next(30, 60);
                Strength = random.Next(30, 60);
                Charisma = random.Next(30, 60);
            }
        }
    }
}
