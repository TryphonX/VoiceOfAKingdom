using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class GameManager
    {
        public List<Game> Games { get; private set; } = new List<Game>();
        private const int PROGRESS_BAR_BOXES = 10;
        public static bool HasGame(List<Game> games, ulong userID) =>
            games.Any(game => game.PlayerID == userID);

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
                .WithTitle($"☀️ Month {game.MonthsInControl + 1} | {game.Date.ToLongDateString()}")
                .AddField(new EmbedFieldBuilder()
                    .WithName("\u200B")
                    .WithValue("\u200B"));

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

            embed.AddField(new EmbedFieldBuilder()
                .WithName("\u200B")
                .WithValue("\u200B"));

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
