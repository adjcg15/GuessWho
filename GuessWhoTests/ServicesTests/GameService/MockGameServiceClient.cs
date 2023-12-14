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
        public void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch)
        {

        }
    }
}
