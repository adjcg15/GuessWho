using GuessWhoDataAccess;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract(CallbackContract = typeof(IChatCallback))]
    public interface IChatService
    {
        [OperationContract]
        Response<bool> SendMessage(string invitationCode, string message);
    }

    [ServiceContract]
    public interface IChatCallback
    {
        [OperationContract]
        void NotifyNewMessage(string message, string senderNickname);
    }
}