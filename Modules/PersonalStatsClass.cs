using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class PersonalStatsClass
    {
        [Range(0, 100)]
        public short Happiness { get; set; }
        [Range(0, 100)]
        public short Charisma { get; set; }

        public PersonalStatsClass()
        {
            Random random = new Random();

            Happiness = (short)random.Next(30, 60);
            Charisma = (short)random.Next(30, 60);
        }

        public PersonalStatsClass(short happiness = 0, short charisma = 0)
        {
            Happiness = happiness;
            Charisma = charisma;
        }

        public static PersonalStatsClass operator +(PersonalStatsClass personalStats, PersonalStatsClass incPersonalStats)
        {
            personalStats.Happiness += incPersonalStats.Happiness;
            personalStats.Happiness = CommonScript.Check0To100Range(personalStats.Happiness);

            personalStats.Charisma += incPersonalStats.Charisma;
            personalStats.Charisma = CommonScript.Check0To100Range(personalStats.Charisma);

            return personalStats;
        }

        public void IncValues(short incHappiness = 0, short incCharisma = 0)
        {
            Happiness += incHappiness;
            Charisma += incCharisma;
        }
    }
}
