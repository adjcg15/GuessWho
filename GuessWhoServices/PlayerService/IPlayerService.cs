using GuessWhoDataAccess;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace GuessWhoServices.PlayerService
{
    [ServiceContract]
    public interface IPlayerService
    {
        Response<List<TopPlayer>> GetBestPlayers(int totalPlayers);
    }
}
