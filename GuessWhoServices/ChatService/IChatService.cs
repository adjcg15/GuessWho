using GuessWhoDataAccess;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract(CallbackContract = typeof(IChatCallback))]
    public interface IChatService
    {
        [OperationContract(IsOneWay = true)]
        void RegisterChatRoom(string ChatRoomCode);

        [OperationContract(IsOneWay = true)]
        void JoinChatRoom(string chatRoomCode);

        [OperationContract(IsOneWay = true)]
        void DeleteChatRoom(string chatRoomCode);

        [OperationContract]
        Response<bool> SendMessage(string chatRoomCode, string message);
    }

    public interface IChatCallback
    {
        [OperationContract]
        void NewMessageReceived(string message);
    }
}
