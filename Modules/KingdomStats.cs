using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class KingdomStats
    {
        [Range(0, 100)]
        public short Folks { get; set; }
        [Range(0, 100)]
        public short Military { get; set; }
        [Range(0, 100)]
        public short Nobles { get; set; }
        [Range(0, 100)]
        public short Wealth { get; set; }

        public KingdomStats()
        {
            Folks = (short)CommonScript.Rng.Next(40, 60);
            Wealth = (short)CommonScript.Rng.Next(40, 60);
            Nobles = (short)CommonScript.Rng.Next(40, 60);
            Military = (short)CommonScript.Rng.Next(40, 60);
        }

        public KingdomStats(short folks = 0, short nobles = 0, short military = 0, short wealth = 0)
        {
            Folks = folks;
            Nobles = nobles;
            Military = military;
            Wealth = wealth;
        }

        public static KingdomStats operator +(KingdomStats kingdomStats, KingdomStats incKingdomStats)
        {
            kingdomStats.Folks = CommonScript.Check0To100Range(kingdomStats.Folks += incKingdomStats.Folks);

            kingdomStats.Nobles = CommonScript.Check0To100Range(kingdomStats.Nobles += incKingdomStats.Nobles);

            kingdomStats.Wealth = CommonScript.Check0To100Range(kingdomStats.Wealth += incKingdomStats.Wealth);

            kingdomStats.Military = CommonScript.Check0To100Range(kingdomStats.Military += incKingdomStats.Military);

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

        public void Inc(short folks = 0, short nobles = 0, short military = 0, short wealth = 0)
        {
            Folks = CommonScript.Check0To100Range(Folks += folks);
            Nobles = CommonScript.Check0To100Range(Nobles += nobles);
            Military = CommonScript.Check0To100Range(Military += military);
            Wealth = CommonScript.Check0To100Range(Wealth += wealth);
        }
    }
}
