using GuessWhoDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoServices
{
    [ServiceContract]
    interface IFriendsService
    {
        [OperationContract]
        Response<bool> SendRequest(int idUserRequester, int idUserRequested);

        [OperationContract]
        Response<List<Friend>> GetFriends(int idUser);

        [OperationContract]
        Response<List<Friend>> GetRequests(int idUser);

        [OperationContract]
        Response<bool> AnswerRequest(int idFriendship, string answer);
    }

    [DataContract]
    public class Friend
    {
        [DataMember]
        public string Nickname { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public byte[] Avatar { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public int IdFriendship {  get; set; }
    }
}
