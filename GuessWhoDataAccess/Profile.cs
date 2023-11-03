using System.Runtime.Serialization;

namespace GuessWhoDataAccess
{
    [DataContract]
    public class Profile
    {
        private string nickName;
        private string fullName;
        private byte[] avatar;
        private string email;
        private string password;
        private int idUser;

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

        [DataMember]
        public int IdUser { get { return idUser; } set { idUser = value; } }
    }
}
