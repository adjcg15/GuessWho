using GuessWhoDataAccess;
using System.Collections.Generic;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract(CallbackContract = typeof(IUsersCallback))]
    interface IUserService
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe();

        [OperationContract(IsOneWay = true)]
        void Unsubscribe();

        [OperationContract]
        List<string> GetActiveUsers();
    }

    [ServiceContract]
    public interface IUsersCallback
    {
        [OperationContract]
        void UserStatusChanged(string userNickname, bool isActive);
    }
}
