using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class Image
    {
        private readonly string value;

        public static string WarriorSide { get { return new Image("https://i.imgur.com/KWEvPkK.png").value; } }
        public static string WomanWithRose { get { return new Image("https://i.imgur.com/qV3c7ej.png").value; } }
        public static string ComicManGoldenArmor { get { return new Image("https://i.imgur.com/yvc3BNe.png").value; } }
        public static string ThrownHelmet { get { return new Image("https://i.imgur.com/GGk5zRV.png").value; } }
        public static string WarriorRaisedFist { get { return new Image("https://i.imgur.com/8fcGW6K.png").value; } }
        public static string PointingSword { get { return new Image("https://i.imgur.com/0Ugz4ds.png").value; } }
        public static string BloodySwordDarkPurple { get { return new Image("https://i.imgur.com/YfT1nJC.png").value; } }
        public static string Bloodbath { get { return new Image("https://i.imgur.com/2fUMEIl.png").value; } }
        public static string RaisedFist { get { return new Image("https://i.imgur.com/Mqu7IHl.png").value; } }
        public static string HitBackOfHelmet { get { return new Image("https://i.imgur.com/SQPfclQ.png").value; } }
        public static string HoldingSwordToTheSky { get { return new Image("https://i.imgur.com/dZNZnPP.png").value; } }
        public static string DroppedSword { get { return new Image("https://i.imgur.com/V1dIY1I.png").value; } }
        public static string WolfShield { get { return new Image("https://i.imgur.com/Hy4g7r6.png").value; } }

        private Image(string value)
        {
            this.value = value;
        }
    }
}
