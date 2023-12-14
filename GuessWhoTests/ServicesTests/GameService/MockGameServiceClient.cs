using GuessWhoDataAccess;
using GuessWhoTests.GameServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoTests.ServicesTests.GameService
{
    public class MockGameServiceClient : IGameServiceCallback
    {
        public bool HasLeftGame { get; set; }
        public bool HasJoinGame { get; set; }
        public bool IsCurrentMatchHost { get; set; }

        public void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch)
        {
            if (isInMatch)
            {
                HasJoinGame = true;
            }
            else
            {
                HasLeftGame = true;
            }
        }
    }
}
