using System;
using System.Collections.Generic;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class Request
    {
        public string Question { get; private set; }
        public Person Person { get; private set; }
        public Game.KingdomStatsClass KingdomStatsOnAccept { get; private set; }
        public Game.KingdomStatsClass KingdomStatsOnReject { get; private set; }
        public Game.PersonalStatsClass PersonalStatsOnAccept { get; private set; }
        public Game.PersonalStatsClass PersonalStatsOnReject { get; private set; }

        public Request(string question, Person person,
            Game.KingdomStatsClass kingdomStatsOnAccept,
            Game.PersonalStatsClass personalStatsOnAccept,
            Game.KingdomStatsClass kingdomStatsOnReject,
            Game.PersonalStatsClass personalStatsOnReject)
        {
            Question = question;
            Person = person;
            KingdomStatsOnAccept = kingdomStatsOnAccept;
            PersonalStatsOnAccept = personalStatsOnAccept;
            KingdomStatsOnReject = kingdomStatsOnReject;
            PersonalStatsOnReject = personalStatsOnReject;
        }
    }
}
