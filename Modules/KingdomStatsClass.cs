using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class KingdomStatsClass
    {
        [Range(0, 100)]
        public short Folks { get; set; }
        [Range(0, 100)]
        public short Military { get; set; }
        [Range(0, 100)]
        public short Nobles { get; set; }
        [Range(0, 100)]
        public short Wealth { get; set; }

        public KingdomStatsClass()
        {
            Random random = new Random();

            Folks = (short)random.Next(40, 60);
            Wealth = (short)random.Next(40, 60);
            Nobles = (short)random.Next(40, 60);
            Military = (short)random.Next(40, 60);
        }

        public KingdomStatsClass(short folks = 0, short nobles = 0, short military = 0, short wealth = 0)
        {
            Folks = folks;
            Nobles = nobles;
            Military = military;
            Wealth = wealth;
        }

        public static KingdomStatsClass operator +(KingdomStatsClass kingdomStats, KingdomStatsClass incKingdomStats)
        {
            kingdomStats.Folks += incKingdomStats.Folks;
            kingdomStats.Folks = CommonScript.Check0To100Range(kingdomStats.Folks);

            kingdomStats.Nobles += incKingdomStats.Nobles;
            kingdomStats.Nobles = CommonScript.Check0To100Range(kingdomStats.Nobles);

            kingdomStats.Wealth += incKingdomStats.Wealth;
            kingdomStats.Wealth = CommonScript.Check0To100Range(kingdomStats.Wealth);

            kingdomStats.Military += incKingdomStats.Military;
            kingdomStats.Military = CommonScript.Check0To100Range(kingdomStats.Military);

            return kingdomStats;
        }

        /// <summary>
        /// Well ALMOST invert them. Inverting them would be too harsh on the player.
        /// </summary>
        public void InvertReputations()
        {
            Folks = (short)Math.Abs(40 - Folks);
            Nobles = (short)Math.Abs(40 - Nobles);
            Military = (short)Math.Abs(40 - Military);
        }

        public void IncValues(short incFolks = 0, short incNobles = 0, short incMilitary = 0, short incWealth = 0)
        {
            Folks += incFolks;
            Nobles += incNobles;
            Military += incMilitary;
            Wealth += incWealth;
        }
    }
}
