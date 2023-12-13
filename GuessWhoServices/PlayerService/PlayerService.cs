﻿using GuessWhoDataAccess;
using System.Collections.Generic;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IPlayerService
    {
        Response<List<TopPlayer>> IPlayerService.GetBestPlayers(string query, int totalPlayers)
        {
            return PlayerDao.GetTopPlayers(query, totalPlayers);
        }
    }
}
