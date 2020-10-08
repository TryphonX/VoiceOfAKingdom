using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceOfAKingdomDiscord.Modules
{
    class GameManager
    {
        public List<Game> Games { get; private set; } = new List<Game>();

        public static bool HasGame(List<Game> games, ulong userID) =>
            games.Any(game => game.PlayerID == userID);
    }
}
