using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract(CallbackContract = typeof(IUsersCallback))]
    interface IUserService
    {
        [OperationContract]
        void Subscribe();

        [OperationContract]
        void Unsubscribe();

        [OperationContract]
        List<ActiveUser> GetActiveUsers();
    }

    [ServiceContract]
    public interface IUsersCallback
    {
        [OperationContract]
        void UserStatusChanged(ActiveUser user, bool isActive);
    }

    [DataContract]
    public class ActiveUser
    {
        [DataMember]
        public string Nickname { get; set; }

        [DataMember]
        public byte[] Avatar { get; set; }
    }
}
