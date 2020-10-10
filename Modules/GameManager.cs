using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;

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
        private const short MILITARY_THRESHOLD = 50;
        private const short FOLK_THRESHOLD = 50;
        private const short NOBLE_THRESHOLD = 50;
        private const short WEALTH_THRESHOLD = 50;

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
                new Game.PersonalStatsClass(happiness: MEDIUM_CHANGE, charisma: -SMALL_CHANGE),
                "Thank you or something.",
                "You will regret this or something.")
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

        public static void EndGame(Game game, bool skipMessage = false)
        {
            try
            {
                if (!skipMessage)
                {
                    int yearsInCommand = ++game.MonthsInControl/12;

                    if (yearsInCommand == 1)
                    {
                        GetGameMessageChannel(game).SendMessageAsync($"Game over. You ruled for 1 year and {game.MonthsInControl%12} months").Wait();
                    }
                    else
                    {
                        GetGameMessageChannel(game).SendMessageAsync($"Game over. You ruled for {yearsInCommand} years and {game.MonthsInControl%12} months").Wait();
                    }

                    Thread.Sleep(CommonScript.TIMEOUT_TIME);
                }
                GetGameGuildChannel(game).DeleteAsync();
            }
            catch (Exception e)
            {
                CommonScript.LogError(e.Message);
            }

            App.GameMgr.Games.Remove(game);
        }

        private static SocketGuildChannel GetGameGuildChannel(Game game)
        {
            foreach (var channel in App.Client.GetGuild(game.GuildID).GetCategoryChannel(Config.GamesCategoryID).Channels)
            {
                if (channel.Id != game.ChannelID)
                    continue;

                return channel;
            }

            return null;
        }

        private static ISocketMessageChannel GetGameMessageChannel(Game game)
        {
            foreach (var channel in App.Client.GetGuild(game.GuildID).GetCategoryChannel(Config.GamesCategoryID).Channels)
            {
                if (channel.Id != game.ChannelID)
                    continue;

                return (ISocketMessageChannel)channel;
            }

            return null;
        }

        /// <summary>
        /// Creates the new month embed. Format: https://i.imgur.com/ZEUIPeR.png
        /// </summary>
        /// <param name="game"></param>
        /// <param name="request"></param>
        /// <returns>The new month embed.</returns>
        public static Embed GetNewMonthEmbed(Game game)
        {
            // Base
            EmbedBuilder embed = new CustomEmbed()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName($"Month {++game.MonthsInControl} | {game.Date.ToLongDateString()}"))
                .WithTitle($"\\{game.CurrentRequest.Person.Icon} {game.CurrentRequest.Person.Name}")
                .WithThumbnailUrl(game.CurrentRequest.Person.ImgUrl)
                .WithColor(game.CurrentRequest.Person.Color)
                .WithDescription(game.CurrentRequest.Question)
                .WithTimestamp(game.Date)
                .AddField(CommonScript.EmptyEmbedField());

            StringBuilder sb = new StringBuilder();
            #region On Accept
            // init sb
            // then add all the stat changes about the request
            // in the case you accept
            sb.Append(GetStatChangesString(game.CurrentRequest.KingdomStatsOnAccept));
            sb.Append(GetStatChangesString(game.CurrentRequest.PersonalStatsOnAccept));

            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName("Changes on Accept")
                .WithValue(sb.ToString()));
            #endregion

            sb.Clear();
            #region On Reject
            // Same things as on accept
            sb.Append(GetStatChangesString(game.CurrentRequest.KingdomStatsOnReject));
            sb.Append(GetStatChangesString(game.CurrentRequest.PersonalStatsOnReject));

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
        private static string GetStatChangesString(object obj)
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
        private static string PrepareStatFieldValue(int stat)
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

        public static void ResolveRequest(Game game, bool accepted)
        {
            CommonScript.DebugLog("init");
            Game.KingdomStatsClass incKingdomStats = accepted ? game.CurrentRequest.KingdomStatsOnAccept : game.CurrentRequest.KingdomStatsOnReject;
            Game.PersonalStatsClass incPersonalStats = accepted ? game.CurrentRequest.PersonalStatsOnAccept : game.CurrentRequest.PersonalStatsOnReject;

            game.KingdomStats += incKingdomStats;
            game.PersonalStats += incPersonalStats;

            GetGameMessageChannel(game).SendMessageAsync(embed: GetResolveRequestEmbed(game, accepted)).Wait();

            if (CheckForBigEvents(game))
                return;

            NextMonth(game);
        }

        private static Embed GetResolveRequestEmbed(Game game, bool accepted)
        {
            return new CustomEmbed()
                .WithColor(game.CurrentRequest.Person.Color)
                .WithThumbnailUrl(game.CurrentRequest.Person.ImgUrl)
                .WithTitle(accepted ? $"{CommonScript.CHECKMARK} Accepted" : $"{CommonScript.NO_ENTRY} Rejected")
                .WithDescription(accepted ? game.CurrentRequest.ResponseOnAccepted : game.CurrentRequest.ResponseOnRejected)
                .WithTimestamp(game.Date)
                .Build();
        }

        private static void NextMonth(Game game)
        {
            game.CurrentRequest = GetRandomRequest();

            game.Date = AddMonthToDate(game.Date);

            GetGameMessageChannel(game).SendMessageAsync(embed: GetNewMonthEmbed(game))
                .ContinueWith(antecedent =>
                {
                    antecedent.Result.AddReactionAsync(new Emoji(CommonScript.CHECKMARK)).Wait();
                    antecedent.Result.AddReactionAsync(new Emoji(CommonScript.NO_ENTRY)).Wait();
                });
        }

        public static Request GetRandomRequest() =>
            App.GameMgr.Requests[new Random().Next(0, App.GameMgr.Requests.Count - 1)];

        private static DateTime AddMonthToDate(DateTime date)
        {
            int monthDays = CommonScript.MonthsWith31Days.Any(monthNum => monthNum == date.Month) ? 31 : 30;
            int minDays = monthDays - date.Day;
            int maxDays = monthDays * 2 - date.Day;

            return date.AddDays(new Random().Next(minDays, maxDays));
        }

        /// <summary>
        /// Returns <see langword="true"/> if you have to skip the next month.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static bool CheckForBigEvents(Game game)
        {
            Random rng = new Random();
            
            // Coup
            if (game.KingdomStats.Military < 20)
            {
                if (rng.Next(0, 100) < MILITARY_THRESHOLD)
                {
                    return CoupStaged(game);
                }
            }
            
            if (game.KingdomStats.Folks < FOLK_THRESHOLD)
            {
                if (rng.Next(0, 100) < 30)
                {
                    return RevolutionStarted(game);
                }

            }
            
            if (game.KingdomStats.Nobles < NOBLE_THRESHOLD)
            {
                if (rng.Next(0, 100) < 30)
                {
                    return AssassinationAttempted(game);
                }

            }
            
            if (game.KingdomStats.Wealth < WEALTH_THRESHOLD)
            {
                if (rng.Next(0, 100) < 30)
                {
                    // No idea
                }
            }

            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> if you have to skip the next month.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static bool CoupStaged(Game game)
        {
            Random rng = new Random();
            const short MURDER_THRESHOLD = 70;

            if (rng.Next(0,100) < MURDER_THRESHOLD)
            {
                // Death
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.DarkPurple)
                    .WithTitle("The military staged a coup!")
                    .WithDescription("You were murdered.")
                    .WithImageUrl(Image.WarriorSide)
                    .Build()).Wait();

                EndGame(game);
                return true;
            }
            else
            {
                // Captured
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.Purple)
                    .WithTitle("The military staged a coup!")
                    .WithDescription("You were captured.")
                    .WithImageUrl(Image.PointingSword)
                    .Build()).Wait();

                // TODO: Maybe in the future pass years captured then break free
                EndGame(game);
                return true;
            }
        }

        /// <summary>
        /// Returns whether to skip next month or not.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static bool RevolutionStarted(Game game)
        {
            Random rng = new Random();
            const short MURDER_THRESHOLD = 20;
            const short CHARISMA_WAY_OUT_THRESHOLD = 50;

            if (rng.Next(0, 100) < MURDER_THRESHOLD)
            {
                // Murder (most likely)
                bool murdered = true;

                StringBuilder storyLine = new StringBuilder("The crowds are forming. Torches, pitchforks and people shouting are setting the mood. " +
                    "You can sense this is not going to end well... Some are throwing tomatoes towards your window. As you open the window, " +
                    "you hear the crowds chanting \"Give us the King's head!\". The guards are forced to let some of them inside. Soon enough, " +
                    "your door opens and the leader of the revolution along with 2 other men enter the room carrying their swords... ");

                if (game.PersonalStats.Charisma > 80 && rng.Next(0, 100) < CHARISMA_WAY_OUT_THRESHOLD)
                {
                    // Charisma backdoor
                    murdered = false;

                    storyLine.Append("After listening to them for a while, you manage to calm them down and get away with all your mistakes, " +
                        "thanks to your charisma. You had to agree to sign their constitution however, which is definitely going to mix things up " +
                        "in your kingdom.");

                    App.GameMgr.Games.Find(listedGame => listedGame.PlayerID == game.PlayerID).KingdomStats.InvertReputations();
                }

                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(murdered ? Color.DarkRed : Color.Orange)
                    .WithTitle("You see a crowds forming outside your window!")
                    .WithDescription(storyLine.ToString())
                    .WithImageUrl(murdered ? Image.Bloodbath : Image.RaisedFist)
                    .Build()).Wait();

                if (murdered)
                    EndGame(game);

                return murdered;
            }
            else
            {
                // Not murdered
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.Orange)
                    .WithTitle("You see crowds forming outside your window!")
                    .WithDescription("The crowds are forming. People are shouting at the top of their lungs. " +
                    "Your nation is not happy with how you've been treating them. You decide to open the window and all you hear is people " +
                    "chanting for a constitution. You have no option, but to give in to their demands. You signed their constitution.")
                    .WithImageUrl(Image.RaisedFist)
                    .Build()).Wait();

                return false;
            }
        }

        private static bool AssassinationAttempted(Game game)
        {
            Random rng = new Random();
            const short CHARISMA_WAY_OUT_THRESHOLD = 20;
            const short FIGHT_IT_OUT_THRESHOLD = 40;

            if (game.PersonalStats.Charisma > 80 && rng.Next(0, 100) < CHARISMA_WAY_OUT_THRESHOLD)
            {
                // Charisma backdoor
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.DarkBlue)
                    .WithTitle("Not the sharpest tool in the shed...")
                    .WithDescription("You've been making quite a lot of enemies in the wealthy circles. Unlike folks, nobles " +
                    "will not be as obvious about their moves... You were hanging out at a tavern with some folks, when you noticed " +
                    "a suspicious figure outside. You leave through the back door and set up an ambush with your guards. " +
                    "You are immediately followed by the suspicious person and soon enough they are surrounded by your guards " +
                    "and folks. You convince them to give away their boss and then jail them. The noble who hired the assassin left " +
                    "the country and nobles will not be attempting anything against you for a while.")
                    .WithImageUrl(Image.HitBackOfHelmet)
                    .Build()).Wait();

                App.GameMgr.Games.Find(listedGame => listedGame.PlayerID == game.PlayerID).KingdomStats.IncValues(incFolks: SMALL_CHANGE, incNobles: MEDIUM_CHANGE);
                App.GameMgr.Games.Find(listedGame => listedGame.PlayerID == game.PlayerID).PersonalStats.IncValues(incHappiness: SMALL_CHANGE);
                return false;
            }
            else if (rng.Next(0, 100) < FIGHT_IT_OUT_THRESHOLD)
            {
                // Fought it out
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.DarkBlue)
                    .WithTitle("Royal mistake...")
                    .WithDescription("You've been making quite a lot of enemies in the wealthy circles. Unlike folks, nobles " +
                    "will not be as obvious about their moves... You were hanging out at a tavern with some folks, when you noticed " +
                    "a suspicious figure outside. You invite them inside, but they seem to ignore you. You slowly walk towards them... They " +
                    "suddenly attempt stabbing you with a dagger, but you manage to disarm them. You both get your swords out. The fight goes on " +
                    "for a while until you manage to tackle the assassin, throwing them behind the bar of the tavern and force them to surrender " +
                    "pointing your sword at their neck. The assassin was decapitated the next day. The medical staff took care of your wounds " +
                    "when you got back. No noble will attempt this again anytime soon.")
                    .WithImageUrl(Image.PointingSword)
                    .Build()).Wait();

                App.GameMgr.Games.Find(listedGame => listedGame.PlayerID == game.PlayerID).KingdomStats.IncValues(incFolks: SMALL_CHANGE, incNobles: MEDIUM_CHANGE);
                App.GameMgr.Games.Find(listedGame => listedGame.PlayerID == game.PlayerID).PersonalStats.IncValues(incHappiness: -MEDIUM_CHANGE);
                return false;
            }
            else
            {
                // Assassinated
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.DarkPurple)
                    .WithTitle("Backstabbed...")
                    .WithDescription("You've been making quite a lot of enemies in the wealthy circles. Unlike folks, nobles " +
                    "will not be as obvious about their moves... And that's exactly what you felt at your back as you were leaving a tavern. " +
                    "An assassin immediately backstabbed you as you got out and ran away before anyone could stop them. No one managed to save you.")
                    .WithImageUrl(Image.BloodySwordDarkPurple)
                    .Build()).Wait();

                EndGame(game);
                return true;
            }
        }
    }
}
