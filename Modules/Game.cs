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
                channel.SendMessageAsync(embed: App.GameMgr.GetNewMonthEmbed(this)).Wait();

                commandHandler.Msg.Channel.SendMessageAsync($"New game started \\➡️ <#{antecedent.Result.Id}>");
                channel.SendMessageAsync(embed: App.GameMgr.GetNewRequestEmbed(App.GameMgr.Requests[new Random().Next(0, App.GameMgr.Requests.Count - 1)]));
            });
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

            public KingdomStatsClass(short folks, short nobles, short military, short wealth)
            {
                Folks = folks;
                Nobles = nobles;
                Military = military;
                Wealth = wealth;
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

            public PersonalStatsClass(short happiness, short charisma)
            {
                Happiness = happiness;
                Charisma = charisma;
            }
        }
    }
}
