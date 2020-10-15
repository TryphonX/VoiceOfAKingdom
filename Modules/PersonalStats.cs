using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class PersonalStats
    {
        [Range(0, 100)]
        public short Happiness { get; set; }
        [Range(0, 100)]
        public short Charisma { get; set; }

        public PersonalStats()
        {
            Happiness = (short)CommonScript.Rng.Next(30, 60);
            Charisma = (short)CommonScript.Rng.Next(30, 60);
        }

        public PersonalStats(short happiness = 0, short charisma = 0)
        {
            Happiness = happiness;
            Charisma = charisma;
        }

        public static PersonalStats operator +(PersonalStats personalStats, PersonalStats incPersonalStats)
        {
            personalStats.Happiness = CommonScript.Check0To100Range(personalStats.Happiness += incPersonalStats.Happiness);

            personalStats.Charisma = CommonScript.Check0To100Range(personalStats.Charisma += incPersonalStats.Charisma);

            return personalStats;
        }

        public void Inc(short happiness = 0, short charisma = 0)
        {
            Happiness = CommonScript.Check0To100Range(Happiness += happiness);
            Charisma = CommonScript.Check0To100Range(Charisma += charisma);
        }
    }
}
