using System.Runtime.Serialization;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract]
    public interface IAuthenticationService
    {
        [OperationContract]
        bool RegisterUser(Profile user);

        [OperationContract]
        Profile Login(string email, string password);

        [OperationContract]
        bool VerifyUserRegisteredByEmail(string email);

        [OperationContract]
        bool VerifyUserRegisteredByNickName(string nickname);
        [OperationContract]
        void Logout(string nickname);
    }

    [DataContract]
    public class Profile
    {
        private string nickName;
        private string fullName;
        private byte[] avatar;
        private string email;
        private string password;

        [DataMember]
        public string NickName { get { return nickName; } set { nickName = value; } }

        [DataMember]
        public string FullName { get { return fullName; } set { fullName = value; } }

        [DataMember]
        public byte[] Avatar { get { return avatar; } set { avatar = value; } }

        [DataMember]
        public string Email { get { return email; } set { email = value; } }

        [DataMember]
        public string Password { get { return password; } set { password = value; } }
    }
}
