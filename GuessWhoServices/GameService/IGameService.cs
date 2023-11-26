using GuessWhoDataAccess;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract(CallbackContract = typeof(IGameCallback))]
    public interface IGameService
    {
        [OperationContract]
        Response<string> CreateMatch(string hostNickname);

        [OperationContract]
        Response<PlayerInMatch> JoinGame(string invitationCode, string nickname);

        [OperationContract(IsOneWay = true)]
        void ExitGame(string invitationCode);

        [OperationContract(IsOneWay = true)]
        void FinishGame(string invitationCode);

        //[OperationContract]
        //Response<bool> SendMessage(string invitationCode, string message);
    }

    [ServiceContract]
    public interface IGameCallback
    {
        [OperationContract]
        void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch);
        //[OperationContract]
        //void NotifyNewMessage(string message, string senderNickname);
    }
}
