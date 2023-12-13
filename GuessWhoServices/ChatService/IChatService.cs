using GuessWhoDataAccess;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract(CallbackContract = typeof(IChatCallback))]
    public interface IChatService
    {
        [OperationContract(IsOneWay = true)]
        void EnterToChatRoom(string chatRoomCode);

        [OperationContract(IsOneWay = true)]
        void LeaveChatRoom(string chatRoomCode);

        [OperationContract]
        Response<bool> SendMessage(string chatRoomCode, string message);
    }

    [ServiceContract]
    public interface IChatCallback
    {
        [OperationContract]
        void NewMessageReceived(string message);
    }
}
