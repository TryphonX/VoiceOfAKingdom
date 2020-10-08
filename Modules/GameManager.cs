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

        public static bool TryGetGame(ulong userID, GameManager gameMgr, out Game game)
        {
            game = null;
            if (HasGame(gameMgr.Games, userID))
            {
                foreach (var gameMgrGame in gameMgr.Games)
                {
                    if (gameMgrGame.PlayerID == userID)
                    {
                        game = gameMgrGame;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
