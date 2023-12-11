using GuessWhoDataAccess;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract(CallbackContract = typeof(IGameCallback))]
    public interface IGameService
    {
        [OperationContract]
        Response<string> CreateMatch(string hostNickname, bool isTournamentMatch);

        [OperationContract]
        Response<PlayerInMatch> JoinGame(string invitationCode, string nickname);

        [OperationContract(IsOneWay = true)]
        void ExitGame(string invitationCode);

        [OperationContract(IsOneWay = true)]
        void FinishGame(string invitationCode);
    }

    [ServiceContract]
    public interface IGameCallback
    {
        [OperationContract]
        void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch);
    }
}
