using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class GameManager
    {
        public List<Game> Games { get; } = new List<Game>();
        private const int PROGRESS_BAR_BOXES = 10;
        public static bool HasGame(List<Game> games, ulong userID) =>
            games.Any(game => game.PlayerID == userID);
        public List<Request> Requests { get; } = new List<Request>();
        private const string UP_ARROW_SMALL = "\\🔼";
        private const string DOWN_ARROW_SMALL = "\\🔻";

        public GameManager()
        {
            Requests.Add(new Request("Some question?", Person.General,
                new Game.KingdomStatsClass(-4, 0, 8, -2),
                new Game.PersonalStatsClass(-4, 1),
                new Game.KingdomStatsClass(4, 0, -8, 2),
                new Game.PersonalStatsClass(4, -1)));
        }

        public static bool TryGetGame(ulong userID, out Game game)
        {
            game = null;
            if (HasGame(App.GameMgr.Games, userID))
            {
                foreach (var gameMgrGame in App.GameMgr.Games)
                {
                    if (gameMgrGame.PlayerID == userID)
                    {
                        game = gameMgrGame;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public static void EndGame(Game game)
        {
            try
            {
                GetGameChannel(game).DeleteAsync();
            }
            catch (Exception e)
            {
                CommonScript.LogError(e.Message);
            }

            App.GameMgr.Games.Remove(game);
        }

        public static SocketGuildChannel GetGameChannel(Game game)
        {
            foreach (var channel in App.Client.GetGuild(game.GuildID).GetCategoryChannel(Config.GamesCategoryID).Channels)
            {
                if (channel.Id != game.ChannelID)
                    continue;

                return channel;
            }

            return null;
        }

        public Embed GetNewMonthEmbed(Game game)
        {
            EmbedBuilder embed = new CustomEmbed()
                .WithTitle($"☀️ Month {++game.MonthsInControl} | {game.Date.ToLongDateString()}")
                .WithDescription("==============================")
                .AddField(CommonScript.EmptyEmbedField());

            #region Folks
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($":banjo: Folks: {game.KingdomStats.Folks}")
                .WithValue(PrepareStatFieldValue(game.KingdomStats.Folks)));
            #endregion

            #region Nobles
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"👑 Nobles: {game.KingdomStats.Nobles}")
                .WithValue(PrepareStatFieldValue(game.KingdomStats.Nobles)));
            #endregion

            embed.AddField(CommonScript.EmptyEmbedField());

            #region Military
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"🛡 Military: {game.KingdomStats.Military}")
                .WithValue(PrepareStatFieldValue(game.KingdomStats.Military)));
            #endregion

            #region Wealth
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($":coin: Wealth: {game.KingdomStats.Wealth}")
                .WithValue(PrepareStatFieldValue(game.KingdomStats.Wealth)));
            #endregion

            embed.AddField(CommonScript.EmptyEmbedField());

            embed.AddField(new EmbedFieldBuilder()
                .WithName("🤔 Personal Info")
                .WithValue("=============================="));

            #region Happiness
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"😄 Happiness: {game.PersonalStats.Happiness}")
                .WithValue(PrepareStatFieldValue(game.PersonalStats.Happiness)));
            #endregion

            #region Charisma
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"🤵‍♂️ Charisma: {game.PersonalStats.Charisma}")
                .WithValue(PrepareStatFieldValue(game.PersonalStats.Charisma)));
            #endregion

            return embed.Build();
        }

        public Embed GetNewRequestEmbed(Request request)
        {
            EmbedBuilder embed = new CustomEmbed()
                .WithTitle(request.Person.Name)
                .WithThumbnailUrl(request.Person.ImageLink)
                .WithColor(request.Person.Color)
                .WithDescription(request.Question)
                .AddField(CommonScript.EmptyEmbedField());

            StringBuilder sb = new StringBuilder();
            foreach (var property in request.KingdomStatsOnAccept.GetType().GetProperties())
            {
                if ((short)property.GetValue(request.KingdomStatsOnAccept) > 0)
                {
                    sb.AppendLine($"{UP_ARROW_SMALL} {property.Name}\n");
                }
                else if ((short)property.GetValue(request.KingdomStatsOnAccept) < 0)
                {
                    sb.AppendLine($"{DOWN_ARROW_SMALL} {property.Name}\n");
                }
            }

            foreach (var property in request.PersonalStatsOnAccept.GetType().GetProperties())
            {
                if ((short)property.GetValue(request.PersonalStatsOnAccept) > 0)
                {
                    sb.AppendLine($"{UP_ARROW_SMALL} {property.Name}\n");
                }
                else if ((short)property.GetValue(request.PersonalStatsOnAccept) < 0)
                {
                    sb.AppendLine($"{DOWN_ARROW_SMALL} {property.Name}\n");
                }
            }

            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("Stats Effects on Accept")
                .WithValue(sb.ToString()));

            sb.Clear();
            foreach (var property in request.KingdomStatsOnReject.GetType().GetProperties())
            {
                if ((short)property.GetValue(request.KingdomStatsOnReject) > 0)
                {
                    sb.AppendLine($"{UP_ARROW_SMALL} {property.Name}\n");
                }
                else if ((short)property.GetValue(request.KingdomStatsOnReject) < 0)
                {
                    sb.AppendLine($"{DOWN_ARROW_SMALL} {property.Name}\n");
                }
            }

            foreach (var property in request.PersonalStatsOnReject.GetType().GetProperties())
            {
                if ((short)property.GetValue(request.PersonalStatsOnReject) > 0)
                {
                    sb.AppendLine($"{UP_ARROW_SMALL} {property.Name}\n");
                }
                else if ((short)property.GetValue(request.PersonalStatsOnReject) < 0)
                {
                    sb.AppendLine($"{DOWN_ARROW_SMALL} {property.Name}\n");
                }
            }

            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("Stats Effects on Reject")
                .WithValue(sb.ToString()));

            return embed.Build();
        }

        private string PrepareStatFieldValue(int stat)
        {
            StringBuilder sb = new StringBuilder("[");
            int roundedStat = CommonScript.RoundToX(stat, PROGRESS_BAR_BOXES);
            for (short i = 0; i < PROGRESS_BAR_BOXES; i++)
            {
                if (i * PROGRESS_BAR_BOXES >= roundedStat)
                {
                    sb.Append("□");
                }
                else
                {
                    sb.Append("■");
                }
            }

            return sb.Append("]").ToString();
        }
    }
}
