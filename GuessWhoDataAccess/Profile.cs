using System.Runtime.Serialization;

namespace GuessWhoDataAccess
{
    [DataContract]
    public class Profile
    {
        [DataMember]
        public string NickName { get ; set ; } 

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public byte[] Avatar { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public int IdUser { get; set; }
    }
}
