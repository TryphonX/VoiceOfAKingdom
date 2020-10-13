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
        public Game.PersonalStatsClass PersonalStatsOnAccept { get; }
        public Game.PersonalStatsClass PersonalStatsOnReject { get; }
        public string ResponseOnAccepted { get; }
        public string ResponseOnRejected { get; }

        public Request(string question, Person person,
            KingdomStatsClass kingdomStatsOnAccept,
            Game.PersonalStatsClass personalStatsOnAccept,
            KingdomStatsClass kingdomStatsOnReject,
            Game.PersonalStatsClass personalStatsOnReject,
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
    }
}
