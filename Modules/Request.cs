using System;
using System.Collections.Generic;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class Request
    {
        public string Question { get; }
        public Person Person { get; }
        public KingdomStatsClass KingdomStatsOnAccept { get; }
        public KingdomStatsClass KingdomStatsOnReject { get; }
        public PersonalStatsClass PersonalStatsOnAccept { get; }
        public PersonalStatsClass PersonalStatsOnReject { get; }
        public string ResponseOnAccepted { get; }
        public string ResponseOnRejected { get; }

        public Request(string question, Person person,
            KingdomStatsClass kingdomStatsOnAccept,
            PersonalStatsClass personalStatsOnAccept,
            KingdomStatsClass kingdomStatsOnReject,
            PersonalStatsClass personalStatsOnReject,
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
