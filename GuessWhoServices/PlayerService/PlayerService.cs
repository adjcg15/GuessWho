using GuessWhoDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoServices.PlayerService
{
    public class PlayerService : IPlayerService
    {
        Response<List<TopPlayer>> IPlayerService.GetBestPlayers(int totalPlayers)
        {
            return PlayerDAO.GetTopPlayers(totalPlayers);
        }
    }
}
