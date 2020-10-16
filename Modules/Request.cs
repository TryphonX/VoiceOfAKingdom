using System;
using System.Collections.Generic;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class Request
    {
        public string Question { get; }
        public Person Person { get; }
        public KingdomStats KingdomStatsOnAccept { get; }
        public KingdomStats KingdomStatsOnReject { get; }
        public PersonalStats PersonalStatsOnAccept { get; }
        public PersonalStats PersonalStatsOnReject { get; }
        public string ResponseOnAccepted { get; }
        public string ResponseOnRejected { get; }
        public Source Src { get; }
        public int Index { get; }

        public Request(string question, Person person,
            KingdomStats kingdomStatsOnAccept,
            PersonalStats personalStatsOnAccept,
            KingdomStats kingdomStatsOnReject,
            PersonalStats personalStatsOnReject,
            string responseOnAccepted,
            string responseOnRejected,
            int index, Source source)
        {
            Question = question;
            Person = person;
            KingdomStatsOnAccept = kingdomStatsOnAccept;
            PersonalStatsOnAccept = personalStatsOnAccept;
            KingdomStatsOnReject = kingdomStatsOnReject;
            PersonalStatsOnReject = personalStatsOnReject;
            ResponseOnAccepted = responseOnAccepted;
            ResponseOnRejected = responseOnRejected;
            Index = index;
            Src = source;
        }

        public enum Source
        {
            Default,
            Custom,
            Mixed
        }

        public override string ToString()
        {
            return $"{Src} {Index}";
        }

        public static Request Parse(string s)
        {
            try
            {
                string[] values = s.Split(' ');
                Source source = Enum.Parse<Source>(values[0]);
                int index = int.Parse(values[1]);

                if (source.Equals(Source.Custom))
                {
                    return GameManager.CustomRequests[index];
                }
                else
                {
                    return GameManager.DefaultRequests[index];
                }
            }
            catch (Exception e)
            {
                CommonScript.LogError(e.Message);
                return null;
            }
        }
    }
}
