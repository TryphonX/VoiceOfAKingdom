using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class CustomEmbed: EmbedBuilder
    {
        public CustomEmbed()
        {
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                .WithText($"👑 Voice of a Kingdom v{CommonScript.Version}");

            WithFooter(footer);
        }
    }
}
