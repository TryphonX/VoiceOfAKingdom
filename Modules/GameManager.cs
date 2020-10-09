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
        private const string UP_ARROW_SMALL = "\\🔼";
        private const string DOWN_ARROW_SMALL = "\\🔻";
        private const string STEADY_ICON = "\\➖";
        private const short SMALL_CHANGE = 4;
        private const short MEDIUM_CHANGE = 9;
        private const short BIG_CHANGE = 12;

        public List<Game> Games { get; } = new List<Game>();
        private const int PROGRESS_BAR_BOXES = 10;
        public static bool HasGame(List<Game> games, ulong userID) =>
            games.Any(game => game.PlayerID == userID);
        public List<Request> Requests { get; } = new List<Request>()
        {
            new Request("Some question?", Person.General,
                new Game.KingdomStatsClass(folks: -MEDIUM_CHANGE, military: BIG_CHANGE, wealth: -SMALL_CHANGE),
                new Game.PersonalStatsClass(happiness: -MEDIUM_CHANGE, charisma: SMALL_CHANGE),
                new Game.KingdomStatsClass(folks: MEDIUM_CHANGE, military: -BIG_CHANGE, wealth: SMALL_CHANGE),
                new Game.PersonalStatsClass(happiness: MEDIUM_CHANGE, charisma: -SMALL_CHANGE))
        };

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

        /// <summary>
        /// Creates the new month embed. Format: https://i.imgur.com/ZEUIPeR.png
        /// </summary>
        /// <param name="game"></param>
        /// <param name="request"></param>
        /// <returns>The new month embed.</returns>
        public Embed GetNewMonthEmbed(Game game, Request request)
        {
            // Base
            EmbedBuilder embed = new CustomEmbed()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName($"Month {++game.MonthsInControl} | {game.Date.ToLongDateString()}"))
                .WithTitle($"\\{request.Person.Icon} {request.Person.Name}")
                .WithThumbnailUrl(request.Person.ImageLink)
                .WithColor(request.Person.Color)
                .WithDescription(request.Question)
                .WithTimestamp(game.Date)
                .AddField(CommonScript.EmptyEmbedField());

            StringBuilder sb = new StringBuilder();
            #region On Accept
            // init sb
            // then add all the stat changes about the request
            // in the case you accept
            sb.Append(GetStatChangesString(request.KingdomStatsOnAccept));
            sb.Append(GetStatChangesString(request.PersonalStatsOnAccept));

            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("Changes on Accept")
                .WithValue(sb.ToString()));
            #endregion

            sb.Clear();
            #region On Reject
            // Same things as on accept
            sb.Append(GetStatChangesString(request.KingdomStatsOnReject));
            sb.Append(GetStatChangesString(request.PersonalStatsOnReject));

            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("Changes on Reject")
                .WithValue(sb.ToString()));
            #endregion

            embed.AddField(CommonScript.EmptyEmbedField());

            #region Folks
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"{Person.Folk.Icon} Folks: {game.KingdomStats.Folks}")
                .WithValue(PrepareStatFieldValue(game.KingdomStats.Folks)));
            #endregion

            #region Nobles
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"{Person.Noble.Icon} Nobles: {game.KingdomStats.Nobles}")
                .WithValue(PrepareStatFieldValue(game.KingdomStats.Nobles)));
            #endregion

            embed.AddField(CommonScript.EmptyEmbedField());

            #region Military
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"{Person.General.Icon} Military: {game.KingdomStats.Military}")
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

        /// <summary>
        /// Creates the string for the stat effects.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string GetStatChangesString(object obj)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var property in obj.GetType().GetProperties())
            {
                if ((short)property.GetValue(obj) > 0)
                {
                    sb.AppendLine($"{UP_ARROW_SMALL} {property.Name}");
                }
                else if ((short)property.GetValue(obj) < 0)
                {
                    sb.AppendLine($"{DOWN_ARROW_SMALL} {property.Name}");
                }
                else
                {
                    sb.AppendLine($"{STEADY_ICON} {property.Name}");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Prepares the Value of the stat field (bar).
        /// </summary>
        /// <param name="stat"></param>
        /// <returns>The string with the linear progress bar of the stat.</returns>
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
