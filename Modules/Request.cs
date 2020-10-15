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

        public Request(string question, Person person,
            KingdomStats kingdomStatsOnAccept,
            PersonalStats personalStatsOnAccept,
            KingdomStats kingdomStatsOnReject,
            PersonalStats personalStatsOnReject,
            string responseOnAccepted,
            string responseOnRejected)
        {
            Question = question;
            Person = person;
            KingdomStatsOnAccept = kingdomStatsOnAccept;
            PersonalStatsOnAccept = personalStatsOnAccept;
            KingdomStatsOnReject = kingdomStatsOnReject;
            PersonalStatsOnReject = personalStatsOnReject;
            ResponseOnAccepted = responseOnAccepted;
            ResponseOnRejected = responseOnRejected;
        }

        public enum Source
        {
            Default,
            Custom,
            Mixed
        }
    }
}
