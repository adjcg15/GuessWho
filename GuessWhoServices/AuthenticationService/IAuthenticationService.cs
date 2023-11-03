using GuessWhoDataAccess;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract]
    public interface IAuthenticationService
    {
        [OperationContract]
        Response<bool> RegisterUser(Profile user);

        [OperationContract]
        Response<Profile> Login(string email, string password);

        [OperationContract]
        Response<Profile> VerifyUserRegisteredByEmail(string email);

        [OperationContract]
        Response<Profile> VerifyUserRegisteredByNickName(string nickname);

        [OperationContract]
        void Logout(string nickname);
    }
}
