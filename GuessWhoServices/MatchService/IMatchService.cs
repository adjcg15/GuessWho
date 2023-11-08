using GuessWhoDataAccess;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract(CallbackContract = typeof(IMatchCallback))]
    public interface IMatchService
    {
        [OperationContract]
        Response<string> CreateMatch(string hostNickname);

        [OperationContract]
        Response<PlayerInMatch> JoinGame(string invitationCode, string nickname);

        [OperationContract]
        Response<bool> ExitGame(string invitationCode);
    }

    [ServiceContract]
    public interface IMatchCallback
    {
        [OperationContract]
        void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch);
    }
}
