using Discord;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class Person
    {
        public string Name { get; }
        public string ImgUrl { get; }
        public Color Color { get; }
        public string Icon { get; }

        private Person(string name, string imgLink, Color color, string icon)
        {
            Name = name;
            ImgUrl = imgLink;
            Color = color;
            Icon = icon;
        }

        public static Person General { get; } = new Person("General Liam Balliol", Image.WarriorRaisedFist, Color.DarkOrange, "🛡");
        public static Person Folk { get; } = new Person("Astarte Mercia", Image.WomanWithRose, Color.Blue, "✊");
        public static Person Noble { get; } = new Person("William Patrick", Image.ComicManGoldenArmor, Color.DarkPurple, "👑");

        public static Person Parse(string type)
        {
            Person person = type switch
            {
                "military" => General,
                "folk" => Folk,
                "noble" => Noble,
                _ => General
            };
            return person;
        }
    }
}
