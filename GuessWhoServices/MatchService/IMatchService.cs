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

        [OperationContract]
        Response<bool> FinishGame(string invitationCode);

        [OperationContract]
        Response<bool> SendMessage(string invitationCode, string message);
    }

    [ServiceContract]
    public interface IMatchCallback
    {
        [OperationContract]
        void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch);
        [OperationContract]
        void NotifyNewMessage(string message, string senderNickname);
    }
}
