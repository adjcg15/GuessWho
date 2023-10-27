using GuessWhoDataAccess;
using System.Collections.Generic;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IPlayerService
    {
        Response<List<TopPlayer>> IPlayerService.GetBestPlayers(int totalPlayers)
        {
            return PlayerDAO.GetTopPlayers(totalPlayers);
        }
    }
}
