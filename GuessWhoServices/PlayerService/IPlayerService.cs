using GuessWhoDataAccess;
using System.Collections.Generic;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract]
    public interface IPlayerService
    {
        [OperationContract]
        Response<List<TopPlayer>> GetBestPlayers(string query, int totalPlayers);
    }
}
