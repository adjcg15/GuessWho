using GuessWhoDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoServices
{
    [ServiceContract]
    interface IProfileService
    {
        [OperationContract]
        Response<bool> UpdateUserProfileImage(byte[] newImage, int idUser);

        [OperationContract]
        Response<bool> UpdateUserNickname(string newNickname, int idUser);

        [OperationContract]
        Response<bool> UpdateUserPassword(string newPassword, int idAccount);

        [OperationContract]
        Response<bool> UpdateUserFullName(string newFullName, int idUser);
    }
}
