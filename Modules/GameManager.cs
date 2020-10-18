using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace VoiceOfAKingdomDiscord.Modules
{
    static class GameManager
    {
        // PATHS
        private const string DEFAULT_REQUESTS_PATH = "./DefaultRequests.xml";
        private const string CUSTOM_REQUESTS_PATH = "./CustomRequests.xml";

        // GAMEPLAY
        private const int MAX_MONTHS_TO_PASS = 5;

        // EMBED
        private const string UP_ARROW_SMALL = "\\🔼";
        private const string DOWN_ARROW_SMALL = "\\🔻";
        private const string STEADY_ICON = "\\➖";
        private const int PROGRESS_BAR_BOXES = 10;

        // PROBABLY REMOVED SOON
        private const short SMALL_CHANGE = 3;
        private const short MEDIUM_CHANGE = 7;
        private const short BIG_CHANGE = 18;

        // THRESHOLDS
        private const short MILITARY_THRESHOLD = 20;
        private const short FOLKS_THRESHOLD = 20;
        private const short NOBLES_THRESHOLD = 20;
        private const short AGE_THRESHOLD = 50;

        private const short HAPPINESS_THRESHOLD = 30;

        // EVENT CHANCES
        private const short COUP_CHANCE = 50;
        private const short REVOLUTION_CHANCE = 50;
        private const short ASSASSINATION_CHANCE = 50;
        private const short SUICIDE_CHANCE = 10;
        private const short DEPRESSION_CHANCE = 40;

        public static List<Game> Games { get; } = new List<Game>();
        public static List<Request> DefaultRequests { get; } = new List<Request>();
        public static List<Request> CustomRequests { get; } = new List<Request>();
        public static bool HasCustomRequests { get; } = CustomRequests.Count != 0;

        public static bool ReloadRequests(bool calledFromCommand = false)
        {
            bool success = false;
            try
            {
                #region Request Node Syntax
                /* <Request>
                 *  <question>Question?</question>
                 *  <type>military/noble/folk</type>
                 *  <accept folk=int noble=int mil=int wealth=int hap=int char=int>Response.</accept>
                 *  <reject folk=int noble=int mil=int wealth=int hap=int char=int>Response.</reject>
                 * </Request>
                 */
                #endregion

                // Default
                if (!calledFromCommand)
                {
                    CommonScript.Log("Loading default requests");

                    ProcessRequestDocument(DEFAULT_REQUESTS_PATH);
                }

                // Custom
                if (File.Exists(CUSTOM_REQUESTS_PATH))
                {
                    if (calledFromCommand)
                        CustomRequests.Clear();

                    CommonScript.Log("Loading custom requests");
                    ProcessRequestDocument(CUSTOM_REQUESTS_PATH);

                    success = true;
                }
                else if (calledFromCommand)
                {
                    CommonScript.LogWarn($"No file found in: {AppDomain.CurrentDomain.BaseDirectory}{CUSTOM_REQUESTS_PATH.Substring(2)}");
                    
                    success = false;
                }
            }
            catch (Exception e)
            {
                CommonScript.LogError(e.Message);
                throw;
            }

            CommonScript.Log("Finished loading requests");
            return success;
        }

        private static void ProcessRequestDocument(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            bool custom = path.Equals(CUSTOM_REQUESTS_PATH);

            #region Init vars
            string question;
            string type;
            short folk;
            short noble;
            short mil;
            short wealth;
            short hap;
            short charisma;
            string responseOnAccepted;
            string responseOnRejected;
            #endregion

            foreach (XmlNode requestNode in doc.DocumentElement)
            {
                if (!requestNode.Name.Equals(XmlNodeEnum.Request))
                    return;

                question = requestNode[XmlNodeEnum.Question].InnerText;
                type = requestNode[XmlNodeEnum.Type].InnerText;

                #region Accept
                folk = ParseValue(requestNode[XmlNodeEnum.Accept].Attributes[XmlAttributeEnum.Folk]);
                noble = ParseValue(requestNode[XmlNodeEnum.Accept].Attributes[XmlAttributeEnum.Noble]);
                mil = ParseValue(requestNode[XmlNodeEnum.Accept].Attributes[XmlAttributeEnum.Military]);
                wealth = ParseValue(requestNode[XmlNodeEnum.Accept].Attributes[XmlAttributeEnum.Wealth]);
                hap = ParseValue(requestNode[XmlNodeEnum.Accept].Attributes[XmlAttributeEnum.Happiness]);
                charisma = ParseValue(requestNode[XmlNodeEnum.Accept].Attributes[XmlAttributeEnum.Charisma]);

                KingdomStats kingdomStatsOnAccept = new KingdomStats(folk, noble, mil, wealth);
                PersonalStats personalStatsOnAccept = new PersonalStats(hap, charisma);

                responseOnAccepted = requestNode[XmlNodeEnum.Accept].InnerText;
                #endregion

                #region Reject
                folk = ParseValue(requestNode[XmlNodeEnum.Reject].Attributes[XmlAttributeEnum.Folk]);
                noble = ParseValue(requestNode[XmlNodeEnum.Reject].Attributes[XmlAttributeEnum.Noble]);
                mil = ParseValue(requestNode[XmlNodeEnum.Reject].Attributes[XmlAttributeEnum.Military]);
                wealth = ParseValue(requestNode[XmlNodeEnum.Reject].Attributes[XmlAttributeEnum.Wealth]);
                hap = ParseValue(requestNode[XmlNodeEnum.Reject].Attributes[XmlAttributeEnum.Happiness]);
                charisma = ParseValue(requestNode[XmlNodeEnum.Reject].Attributes[XmlAttributeEnum.Charisma]);

                KingdomStats kingdomStatsOnReject = new KingdomStats(folk, noble, mil, wealth);
                PersonalStats personalStatsOnReject = new PersonalStats(hap, charisma);

                responseOnRejected = requestNode[XmlNodeEnum.Reject].InnerText;
                #endregion

                if (!custom)
                {
                    DefaultRequests.Add(new Request(question, Person.Parse(type),
                        kingdomStatsOnAccept, personalStatsOnAccept,
                        kingdomStatsOnReject, personalStatsOnReject,
                        responseOnAccepted, responseOnRejected,
                        DefaultRequests.Count, Request.Source.Default));
                }
                else
                {
                    CustomRequests.Add(new Request(question, Person.Parse(type),
                        kingdomStatsOnAccept, personalStatsOnAccept,
                        kingdomStatsOnReject, personalStatsOnReject,
                        responseOnAccepted, responseOnRejected,
                        CustomRequests.Count, Request.Source.Custom));
                }
            }
        }

        #region Fake Enums
        private class XmlNodeEnum
        {
            public static string Request => "Request";
            public static string Question => "question";
            public static string Type => "type";
            public static string Accept => "accept";
            public static string Reject => "reject";
        }

        private class XmlAttributeEnum
        {
            public static string Folk => "folk";
            public static string Noble => "noble";
            public static string Military => "mil";
            public static string Wealth => "wealth";
            public static string Happiness => "hap";
            public static string Charisma => "char";
        }
        #endregion

        private static short ParseValue(XmlAttribute attribute) =>
            attribute != null ? short.Parse(attribute.Value) : (short)0;

        public static bool TryGetGame(ulong userID, out Game game)
        {
            game = null;
            if (HasGame(userID))
            {
                foreach (var gameMgrGame in Games)
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

        public static bool Save(Game game)
        {
            if (game.IsDead)
            {
                return false;
            }

            try
            {
                XmlDocument doc = new XmlDocument();

                // Doc Element
                XmlElement root = doc.CreateElement("Game");
                doc.AppendChild(root);

                doc.InsertBefore(doc.CreateXmlDeclaration("1.0", "UTF-8", null), root);

                foreach (var property in game.GetType().GetProperties())
                {
                    // Skip these
                    if (property.Name.Equals("ChannelID") ||
                        property.Name.Equals("IsDead") ||
                        property.Name.Equals("Age") ||
                        property.Name.Equals("PlayerID"))
                        continue;

                    XmlElement element = doc.CreateElement(property.Name);

                    element.AppendChild(doc.CreateTextNode(property.GetValue(game).ToString()));

                    root.AppendChild(element);
                }

                if (!Directory.Exists("./saves"))
                {
                    Directory.CreateDirectory("./saves");
                }

                doc.Save($"./saves/{game.PlayerID}.xml");

                return true;
            }
            catch (Exception e)
            {
                CommonScript.LogError(e.Message);
                return false;
            }
        }

        public static async void Load(CommandHandler cmdHandler)
        {
            if (File.Exists($"./saves/{cmdHandler.Msg.Author.Id}.xml"))
            {
                // load
                XmlDocument doc = new XmlDocument();
                doc.Load($"./saves/{cmdHandler.Msg.Author.Id}.xml");

                Games.Add(Game.Parse(doc.DocumentElement, cmdHandler));
            }
            else
            {
                await cmdHandler.Msg.Channel.SendMessageAsync("You have no save.");
            }
        }

        public static void SendEndGameMsg(Game game)
        {
            try
            {
                game.IsDead = true;

                int yearsInCommand = game.MonthsInControl / 12;
                int monthsInCommand = game.MonthsInControl % 12;

                string yearsWord = yearsInCommand != 1 ? "years" : "year";
                string monthsWords = monthsInCommand != 1 ? "months" : "month";

                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.Teal)
                    .WithTitle("Game over.")
                    .WithDescription($"You ruled for {yearsInCommand} {yearsWord} and {monthsInCommand} {monthsWords}.")
                    .WithThumbnailUrl(Image.WolfShield)
                    .AddField(new EmbedFieldBuilder()
                        .WithName("End game?")
                        .WithValue("Hit the reaction below to confirm you've read this message."))
                    .Build())
                    .ContinueWith(antecedent =>
                    {
                        antecedent.Result.AddReactionAsync(new Emoji(CommonScript.UnicodeAccept));
                    });
            }
            catch (Exception e)
            {
                CommonScript.LogError(e.Message);
            }
        }

        /// <summary>
        /// Returns if the game ended successfully.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static bool EndGame(Game game)
        {
            bool success;
            try
            {
                success = true;
                GetGameGuildChannel(game).DeleteAsync();
                Games.Remove(game);
            }
            catch (Exception e)
            {
                success = false;
                CommonScript.LogError(e.Message);
            }

            return success;
        }

        /// <summary>
        /// Returns the game's GUILD channel.
        /// <para>Only useful for things like deleting the channel, permissions, etc.</para>
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static SocketGuildChannel GetGameGuildChannel(Game game) =>
            (SocketGuildChannel)App.Client.GetChannel(game.ChannelID);

        /// <summary>
        /// Returns the game's MESSAGE channel.
        /// <para>Useful for things like sending messages, adding reactions, etc</para>
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static ISocketMessageChannel GetGameMessageChannel(Game game) =>
            (ISocketMessageChannel)App.Client.GetChannel(game.ChannelID);

        /// <summary>
        /// Creates the new month embed. Format: https://i.imgur.com/ZEUIPeR.png
        /// </summary>
        /// <param name="game"></param>
        /// <param name="request"></param>
        /// <returns>The new month embed.</returns>
        public static Embed GetNewRequestEmbed(Game game,
            KingdomStats kingdomStatsChanges = null,
            PersonalStats personalStatsChanges = null)
        {
            bool isFirstMonthOrLoaded = game.MonthsInControl == 0;

            // Check for load
            if (!isFirstMonthOrLoaded &&
                (kingdomStatsChanges == null ||
                personalStatsChanges == null))
            {
                isFirstMonthOrLoaded = true;
            }

            // Base
            EmbedBuilder embed = new CustomEmbed()
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName($"Month {game.MonthsInControl+1} | {game.Date.ToLongDateString()} | {game.Age} Years old"))
                .WithTitle($"\\{game.CurrentRequest.Person.Icon} {game.CurrentRequest.Person.Name}")
                .WithThumbnailUrl(game.CurrentRequest.Person.ImgUrl)
                .WithColor(game.CurrentRequest.Person.Color)
                .WithDescription(game.CurrentRequest.Question)
                .WithTimestamp(game.Date)
                .AddField(CommonScript.EmptyEmbedField);

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
            sb.Clear();

            embed.AddField(CommonScript.EmptyEmbedField);

            #region Folks 
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"{Person.Folk.Icon} Folks: {game.KingdomStats.Folks}" +
                $"{GetChangeEmoji(isFirstMonthOrLoaded, !isFirstMonthOrLoaded ? kingdomStatsChanges.Folks : (short)0)}")
                .WithValue(PrepareStatFieldValue(game.KingdomStats.Folks)));
            #endregion

            #region Nobles
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"{Person.Noble.Icon} Nobles: {game.KingdomStats.Nobles}" +
                $"{GetChangeEmoji(isFirstMonthOrLoaded, !isFirstMonthOrLoaded ? kingdomStatsChanges.Nobles : (short)0)}")
                .WithValue(PrepareStatFieldValue(game.KingdomStats.Nobles)));
            #endregion

            embed.AddField(CommonScript.EmptyEmbedField);

            #region Military
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"{Person.General.Icon} Military: {game.KingdomStats.Military}" +
                $"{GetChangeEmoji(isFirstMonthOrLoaded, !isFirstMonthOrLoaded ? kingdomStatsChanges.Military : (short)0)}")
                .WithValue(PrepareStatFieldValue(game.KingdomStats.Military)));
            #endregion

            #region Wealth
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($":coin: Wealth: {game.KingdomStats.Wealth}" +
                $"{GetChangeEmoji(isFirstMonthOrLoaded, !isFirstMonthOrLoaded ? kingdomStatsChanges.Wealth : (short)0)}")
                .WithValue(PrepareStatFieldValue(game.KingdomStats.Wealth)));
            #endregion

            embed.AddField(CommonScript.EmptyEmbedField);

            embed.AddField(new EmbedFieldBuilder()
                .WithName("🤔 Personal Info")
                .WithValue("=============================="));

            #region Happiness
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"😄 Happiness: {game.PersonalStats.Happiness}" +
                $"{GetChangeEmoji(isFirstMonthOrLoaded, !isFirstMonthOrLoaded ? personalStatsChanges.Happiness : (short)0)}")
                .WithValue(PrepareStatFieldValue(game.PersonalStats.Happiness)));
            #endregion

            #region Charisma
            embed.AddField(new EmbedFieldBuilder()
                .WithIsInline(true)
                .WithName($"🤵‍♂️ Charisma: {game.PersonalStats.Charisma}" +
                $"{GetChangeEmoji(isFirstMonthOrLoaded, !isFirstMonthOrLoaded ? personalStatsChanges.Charisma : (short)0)}")
                .WithValue(PrepareStatFieldValue(game.PersonalStats.Charisma)));
            #endregion

            return embed.Build();
        }

        private static string GetChangeEmoji(bool isFirstMonth, short change)
        {
            if (isFirstMonth)
                return string.Empty;

            if (change > 0)
                return $" {UP_ARROW_SMALL}";
            else if (change < 0)
                return $" {DOWN_ARROW_SMALL}";
            else
                return $" {STEADY_ICON}";
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
            if (!game.IsCaptured)
            {
                KingdomStats incKingdomStats = accepted
                    ? game.CurrentRequest.KingdomStatsOnAccept : game.CurrentRequest.KingdomStatsOnReject;
                PersonalStats incPersonalStats = accepted
                    ? game.CurrentRequest.PersonalStatsOnAccept : game.CurrentRequest.PersonalStatsOnReject;

                game.KingdomStats += incKingdomStats;
                game.PersonalStats += incPersonalStats;

                GetGameMessageChannel(game).SendMessageAsync(embed: GetResolveRequestEmbed(game, accepted)).Wait();
            }

            if (CheckForEvents(game))
                return;

            Advance(game, accepted);
        }

        private static Embed GetResolveRequestEmbed(Game game, bool accepted)
        {
            return new CustomEmbed()
                .WithColor(game.CurrentRequest.Person.Color)
                .WithThumbnailUrl(game.CurrentRequest.Person.ImgUrl)
                .WithTitle(accepted ? $"{CommonScript.UnicodeAccept} Accepted" : $"{CommonScript.UnicodeReject} Rejected")
                .WithDescription(accepted ? game.CurrentRequest.ResponseOnAccepted : game.CurrentRequest.ResponseOnRejected)
                .WithTimestamp(game.Date)
                .Build();
        }

        private static void Advance(Game game, bool accepted)
        {
            // Birthday Event
            if (game.Date.Month == game.BirthDate.Month)
            {
                BirthdayEvent(game);
            }

            if (game.IsCaptured)
            {
                // Captured
                AddYearsToDate(game, 1);

                game.PersonalStats.Inc(happiness: -SMALL_CHANGE);

                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.DarkBlue)
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithName($"Months in prison: {game.MonthsCaptured} | {game.Date.ToLongDateString()} | {game.Age} Years old"))
                    .WithTitle("Still captured.")
                    .WithDescription("Yet another year in prison.")
                    .AddField(new EmbedFieldBuilder()
                        .WithName("🤔 Personal Info")
                        .WithValue("=============================="))
                    .AddField(new EmbedFieldBuilder()
                        .WithIsInline(true)
                        .WithName($"😄 Happiness: {game.PersonalStats.Happiness}" +
                        $"{GetChangeEmoji(false, -SMALL_CHANGE)}")
                        .WithValue(PrepareStatFieldValue(game.PersonalStats.Happiness)))
                    .AddField(new EmbedFieldBuilder()
                        .WithIsInline(true)
                        .WithName($"🤵‍♂️ Charisma: {game.PersonalStats.Charisma}" +
                        $"{GetChangeEmoji(false, 0)}")
                        .WithValue(PrepareStatFieldValue(game.PersonalStats.Charisma)))
                    .AddField(new EmbedFieldBuilder()
                        .WithName("Pass another year in jail.")
                        .WithValue("Use the reaction below to proceed to next year."))
                    .WithThumbnailUrl(Image.Guards)
                    .WithTimestamp(game.Date)
                    .Build())
                    .ContinueWith(antecedent =>
                    {
                        antecedent.Result.AddReactionAsync(new Emoji(CommonScript.UnicodeAccept));
                    });
            }
            else
            {
                // Free
                KingdomStats cachedKingdomChanges = accepted ? game.CurrentRequest.KingdomStatsOnAccept : game.CurrentRequest.KingdomStatsOnReject;
                PersonalStats cachedPersonalChanges = accepted ? game.CurrentRequest.PersonalStatsOnAccept : game.CurrentRequest.PersonalStatsOnReject;
                game.CurrentRequest = GetRandomRequest(game.RequestSource);

                // Add 1 to X months (5 at the time of addition)
                AddMonthsToDate(game, CommonScript.Rng.Next(1, MAX_MONTHS_TO_PASS));

                GetGameMessageChannel(game).SendMessageAsync(embed: GetNewRequestEmbed(game, cachedKingdomChanges, cachedPersonalChanges))
                    .ContinueWith(antecedent =>
                    {
                        // Block answers if you don't have the money for them
                        if (game.KingdomStats.Wealth + game.CurrentRequest.KingdomStatsOnAccept.Wealth >= 0)
                            antecedent.Result.AddReactionAsync(new Emoji(CommonScript.UnicodeAccept)).Wait();

                        if (game.KingdomStats.Wealth + game.CurrentRequest.KingdomStatsOnReject.Wealth >= 0)
                            antecedent.Result.AddReactionAsync(new Emoji(CommonScript.UnicodeReject));
                    });
            }
        }

        public static Request GetRandomRequest(Request.Source source)
        {
            if (source != Request.Source.Default && HasCustomRequests)
            {       
                if (source == Request.Source.Custom || CommonScript.GetRandomPercentage() < 50)
                {
                    // Custom (the rng is the chances of custom when in mixed)
                    return CustomRequests[CommonScript.Rng.Next(0, CustomRequests.Count - 1)];
                }
                else
                {
                    // Default
                    return DefaultRequests[CommonScript.Rng.Next(0, DefaultRequests.Count - 1)];
                }
            }
            else
            {
                return DefaultRequests[CommonScript.Rng.Next(0, DefaultRequests.Count - 1)];
            }
        }

        public static bool HasGame(ulong userID) =>
            Games.Any(game => game.PlayerID == userID);

        private static void AddMonthsToDate(Game game, int monthsToAdd)
        {
            // Adding months
            game.Date = game.Date.AddMonths(monthsToAdd);
            
            if (game.IsCaptured)
            {
                game.MonthsCaptured += monthsToAdd;
            }
            else
            {
                game.MonthsInControl += monthsToAdd;
            }

            // Randomizing day
            int monthDays = CommonScript.MonthsWith31Days.Any(monthNum => monthNum == game.Date.Month) ? 31 : 30;

            if (game.Date.Month == 2)
            {
                if (game.Date.Year % 4 == 0)
                {
                    monthDays = 29;
                }
                else
                {
                    monthDays = 28;
                }
            }

            int minDays = -game.Date.Day + 1;
            int maxDays = monthDays - game.Date.Day;

            game.Date = game.Date.AddDays(CommonScript.Rng.Next(minDays, maxDays));

            UpdateAge(game);
        }

        public static void UpdateAge(Game game)
        {
            game.Age = game.Date.DayOfYear >= game.BirthDate.DayOfYear
                ? (short)(game.Date.Year - game.BirthDate.Year)
                : (short)(game.Date.Year - game.BirthDate.Year - 1);
        }

        /// <summary>
        /// Returns <see langword="true"/> if you're dying.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static bool CheckForEvents(Game game)
        {
            bool died = false;

            if (game.Age >= AGE_THRESHOLD && CommonScript.GetRandomPercentage() < CommonScript.RoundToX(game.Age) / 8)
            {
                return DiedOfAge(game);
            }

            // Depression
            if (game.PersonalStats.Happiness < HAPPINESS_THRESHOLD)
            {
                died = DepressionEvent(game);
            }

            // Small check for depression event
            // Maybe other event too in the future
            if (died)
            {
                return true;
            }

            // Breaking free
            if (game.IsCaptured)
            {
                return PrisonEvent(game);
            }

            // Coup
            if (game.KingdomStats.Military < MILITARY_THRESHOLD && CommonScript.GetRandomPercentage() < COUP_CHANCE)
            {
                return CoupStaged(game);
            }
            else if (game.KingdomStats.Folks < FOLKS_THRESHOLD && CommonScript.GetRandomPercentage() < REVOLUTION_CHANCE)
            {
                return RevolutionStarted(game);
            }
            else if (game.KingdomStats.Nobles < NOBLES_THRESHOLD && CommonScript.GetRandomPercentage() < ASSASSINATION_CHANCE)
            {
                return AssassinationAttempted(game);
            }
            else if (game.KingdomStats.Wealth == 0)
            {
                return ReachedBankruptcy(game);
            }

            return false;
        }

        private static bool DiedOfAge(Game game)
        {
            GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                   .WithColor(Color.DarkBlue)
                   .WithTitle("Died of age.")
                   .WithImageUrl(Image.DroppedSword)
                   .Build()).Wait();
            SendEndGameMsg(game);
            return true;
        }

        private static bool DepressionEvent(Game game)
        {
            // The chance to suicide becomes bigger the lower the happiness is.
            // SUICIDE_CHANCE == 10
            // Happines == 30, actual suicide chance is 0
            // Happiness == 0, actual suicide chance is 30
            bool suicided = CommonScript.GetRandomPercentage() < SUICIDE_CHANCE * (3 - CommonScript.RoundToX(game.PersonalStats.Happiness) / 10);

            if (!suicided)
            {
                // Not dying
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.DarkBlue)
                    .WithTitle("You are depressed.")
                    .WithDescription("Recent events have really brought you down. You might want to look into this " +
                    "before it becomes an even more serious issue.")
                    .WithImageUrl(Image.WarriorSide)
                    .Build()).Wait();
            }
            else if (CommonScript.GetRandomPercentage() < DEPRESSION_CHANCE)
            {
                // Dying
                if (game.IsCaptured)
                {
                    GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                        .WithColor(Color.DarkBlue)
                        .WithTitle("It was too much.")
                        .WithDescription($"{game.MonthsCaptured / 12} years and {game.MonthsCaptured % 12} in prison really took a toll on you... " +
                        "You couldn't sleep at night. You kept getting flashbacks of the day the military staged that coup and living " +
                        "in a prison cell did not help alleviate the pain you felt. You just couldn't handle it anymore. " +
                        "You hung yourself in your prison cell.")
                        .WithImageUrl(Image.TiedLegs)
                        .Build()).Wait();
                }
                else
                {
                    GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                        .WithColor(Color.DarkBlue)
                        .WithTitle("It was too much.")
                        .WithDescription("Recent events had really brought you down... You decided to take the easy way out and hang yourself.")
                        .WithImageUrl(Image.TiedLegs)
                        .Build()).Wait();
                }

                SendEndGameMsg(game);
            }

            return suicided;
        }

        private static void BirthdayEvent(Game game)
        {
            if (game.IsCaptured)
            {
                // Don't get to celebrate
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.DarkBlue)
                    .WithTitle("Just like I wanted it.")
                    .WithDescription("Looks like you'll be celebrating your birthday with your cell's rats.")
                    .WithImageUrl(Image.WarriorSide)
                    .Build()).Wait();

                game.PersonalStats.Inc(happiness: -BIG_CHANGE);
            }
            else
            {
                // Celebrating
                StringBuilder sb = new StringBuilder("It's your birthday again. As always, you decide to make a huge, fancy event to celebrate it. ");

                KingdomStats incKingdomStats = new KingdomStats(nobles: SMALL_CHANGE, wealth: -SMALL_CHANGE);
                bool isEventRuined = false;

                if (game.KingdomStats.Wealth < 30)
                {
                    incKingdomStats.Folks = -MEDIUM_CHANGE;
                    incKingdomStats.Military = -SMALL_CHANGE;
                    sb.Append($"{Person.General.Name} informs you that he and a few of his men " +
                        "are disappointed with how you've chosen to use the Kingdom's money at a time like this. " +
                        "The event ends too soon, after a big group of folks gathers to protest for the \"Waste of the Kingdom's money\" " +
                        "during a \"crisis\". In retrospect, maybe you should've held back on the money spending a bit. " +
                        "You end up with a ruined party.");

                    game.PersonalStats.Inc(happiness: -MEDIUM_CHANGE);
                    isEventRuined = true;
                }
                else if (game.KingdomStats.Folks >= 50)
                {
                    incKingdomStats.Folks = SMALL_CHANGE;
                    sb.Append("Everyone enjoyed their time celebrating your birthday.");
                }
                else
                {
                    incKingdomStats.Folks = -SMALL_CHANGE;
                    sb.Append("Given that Folks already disliked you, they're not happy you spent money on an event like this. " +
                        "However, it doesn't really bother you.");
                }

                // Celebrating
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(isEventRuined ? Color.DarkBlue : Color.Green)
                    .WithTitle(isEventRuined ? "We'll try again next year." : "Birthday time!")
                    .WithDescription(sb.ToString())
                    .WithImageUrl(isEventRuined ? Image.PersonShouting : Image.WolfShield)
                    .Build()).Wait();

                game.PersonalStats.Inc(happiness: +SMALL_CHANGE);
                game.KingdomStats += incKingdomStats;
            }
        }

        private static bool PrisonEvent(Game game)
        {
            const short JAIL_BREAK_CHANCE = 20;
            const short EXECUTION_CHANCE = 40;

            if (CommonScript.GetRandomPercentage() < JAIL_BREAK_CHANCE)
            {
                // Breaking free
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.Green)
                    .WithTitle("You managed to break free.")
                    .WithDescription("Some of the inmates have been planning their jail break for some time now. " +
                    $"On {game.Date.ToLongDateString()}, all the inmates start a riot. Guards and inmates dead left and right. " +
                    "You are forced to get your hands dirty to save yourself and some of the inmates you bonded with over the years. " +
                    "You manage to escape and, with the assist of the inmates, regain your position as King. The folks are quite happy " +
                    "with your actions. The nobles appreciate their freedom and the part of the military that was in favor of the coup... " +
                    "was dealt with.")
                    .WithImageUrl(Image.RaisedFist)
                    .Build()).Wait();

                game.KingdomStats.Inc(folks: MEDIUM_CHANGE, nobles: SMALL_CHANGE, military: MEDIUM_CHANGE, wealth: SMALL_CHANGE);
                game.PersonalStats.Inc(happiness: BIG_CHANGE, charisma: SMALL_CHANGE);

                return false;
            }
            else if (CommonScript.GetRandomPercentage() < Math.Abs(EXECUTION_CHANCE - 10 * game.MonthsCaptured))
            {
                // Death
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.DarkRed)
                    .WithTitle("The military decided to execute you.")
                    .WithDescription("You were executed. Your body was fed to the military's dogs.")
                    .WithImageUrl(Image.ThrownHelmet)
                    .Build()).Wait();

                SendEndGameMsg(game);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns <see langword="true"/> if you're dying.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static bool CoupStaged(Game game)
        {
            const short MURDER_THRESHOLD = 70;

            if (CommonScript.Rng.Next(0,100) < MURDER_THRESHOLD)
            {
                // Death
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.DarkRed)
                    .WithTitle("The military staged a coup!")
                    .WithDescription("You were murdered.")
                    .WithImageUrl(Image.WarriorSide)
                    .Build()).Wait();

                SendEndGameMsg(game);
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

                game.IsCaptured = true;
                game.MonthsCaptured = 0;

                return false;
            }
        }

        private static void AddYearsToDate(Game game, int yearsToAdd)
        {
            // 1 year - current month (Janurary of next year)
            // 2 years - current month (December of next year)
            int min = yearsToAdd*12 - game.Date.Month;
            int max = yearsToAdd*12 + 12 - game.Date.Month;

            // Use this function to make sure we're consistent
            AddMonthsToDate(game, CommonScript.Rng.Next(min, max));
        }

        /// <summary>
        /// Returns whether you're dying or not.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static bool RevolutionStarted(Game game)
        {
            const short MURDER_THRESHOLD = 20;
            const short CHARISMA_WAY_OUT_THRESHOLD = 50;

            if (CommonScript.GetRandomPercentage() < MURDER_THRESHOLD)
            {
                // Murder (most likely)
                bool murdered = true;

                StringBuilder storyLine = new StringBuilder("The crowds are forming. Torches, pitchforks and people shouting are setting the mood. " +
                    "You can sense this is not going to end well... Some are throwing tomatoes towards your window. As you open the window, " +
                    "you hear the crowds chanting \"Give us the King's head!\". The guards are forced to let some of them inside. Soon enough, " +
                    "your door opens and the leader of the revolution along with 2 other men enter the room carrying their swords... ");

                if (game.PersonalStats.Charisma > 80 && CommonScript.GetRandomPercentage() < CHARISMA_WAY_OUT_THRESHOLD)
                {
                    // Charisma backdoor
                    murdered = false;

                    storyLine.Append("After listening to them for a while, you manage to calm them down and get away with all your mistakes, " +
                        "thanks to your charisma. You had to agree to sign their constitution however, which is definitely going to mix things up " +
                        "in your kingdom.");

                    Games.Find(listedGame => listedGame.PlayerID == game.PlayerID).KingdomStats.InvertReputations();
                }

                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(murdered ? Color.DarkRed : Color.Orange)
                    .WithTitle("You see a crowds forming outside your window!")
                    .WithDescription(storyLine.ToString())
                    .WithImageUrl(murdered ? Image.Bloodbath : Image.RaisedFist)
                    .Build()).Wait();

                if (murdered)
                    SendEndGameMsg(game);

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

                Games.Find(listedGame => listedGame.PlayerID == game.PlayerID).KingdomStats.InvertReputations();
                return false;
            }
        }

        private static bool AssassinationAttempted(Game game)
        {
            const short CHARISMA_WAY_OUT_THRESHOLD = 20;
            const short FIGHT_IT_OUT_THRESHOLD = 40;

            if (game.PersonalStats.Charisma > 80 && CommonScript.GetRandomPercentage() < CHARISMA_WAY_OUT_THRESHOLD)
            {
                // Charisma backdoor
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.Blue)
                    .WithTitle("Not the sharpest tool in the shed...")
                    .WithDescription("You've been making quite a lot of enemies in the wealthy circles. Unlike folks, nobles " +
                    "will not be as obvious about their moves... You were hanging out at a tavern with some folks, when you noticed " +
                    "a suspicious figure outside. You leave through the back door and set up an ambush with your guards. " +
                    "You are immediately followed by the suspicious person and soon enough they are surrounded by your guards " +
                    "and folks. You convince them to give away their boss and then jail them. The noble who hired the assassin left " +
                    "the country and nobles will not be attempting anything against you for a while.")
                    .WithImageUrl(Image.HitBackOfHelmet)
                    .Build()).Wait();

                Games.Find(listedGame => listedGame.PlayerID == game.PlayerID).KingdomStats
                    .Inc(folks: SMALL_CHANGE, nobles: MEDIUM_CHANGE);
                Games.Find(listedGame => listedGame.PlayerID == game.PlayerID).PersonalStats.
                    Inc(happiness: SMALL_CHANGE);
                return false;
            }
            else if (CommonScript.GetRandomPercentage() < FIGHT_IT_OUT_THRESHOLD)
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

                Games.Find(listedGame => listedGame.PlayerID == game.PlayerID).KingdomStats
                    .Inc(folks: SMALL_CHANGE, nobles: MEDIUM_CHANGE);
                Games.Find(listedGame => listedGame.PlayerID == game.PlayerID).PersonalStats
                    .Inc(happiness: -MEDIUM_CHANGE);
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

                SendEndGameMsg(game);
                return true;
            }
        }

        private static bool ReachedBankruptcy(Game game)
        {
            if (game.KingdomStats.Military > 70 && CommonScript.GetRandomPercentage() < 50)
            {
                // Got ally
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.Blue)
                    .WithTitle("Close call.")
                    .WithDescription("The ecenomy was getting worse by the minute. The nation's treasury was almost empty. " +
                    "You could see the end getting closer... You spent days and nights thinking about ways to get the nation's " +
                    "economy back to its glorious days with no success. The knocking on your door wakes you up. A neighboring nation " +
                    "is interested in getting you as their ally due to your powerful military. Of course, nothing in life comes " +
                    "without a cost. They are willing to pay you to be on their side.")
                    .WithImageUrl(Image.HoldingSwordToTheSky)
                    .Build()).Wait();

                Games.Find(listedGame => listedGame.PlayerID == game.PlayerID).KingdomStats
                    .Inc(folks: -SMALL_CHANGE, military: -SMALL_CHANGE, wealth: BIG_CHANGE);

                Games.Find(listedGame => listedGame.PlayerID == game.PlayerID).PersonalStats
                    .Inc(happiness: BIG_CHANGE, charisma: SMALL_CHANGE);
                return false;
            }
            else
            {
                // Bankruptcy
                GetGameMessageChannel(game).SendMessageAsync(embed: new CustomEmbed()
                    .WithColor(Color.DarkRed)
                    .WithTitle("The treasury is empty!")
                    .WithDescription("There is nothing left in the national treasury. We've used up all our gold and we " +
                    "have to officially declare bankruptcy. You know people will not like this, so you decide to take the " +
                    "easy way out.")
                    .WithImageUrl(Image.DroppedSword)
                    .Build()).Wait();

                SendEndGameMsg(game);
                return true;
            }
        }
    }
}
