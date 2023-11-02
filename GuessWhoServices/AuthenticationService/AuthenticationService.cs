using GuessWhoDataAccess;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IAuthenticationService
    {
        public Response<Profile> Login(string email, string password)
        {
            GuessWhoDataAccess.Response<Profile> response = new GuessWhoDataAccess.Response<Profile>()
            {
                StatusCode = ResponseStatus.OK,
                Value = null
            };

            Account account = UserDAO.VerifyUserSession(email, password);
            if (account == null)
            {
                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
                return response;
            }

            User user = UserDAO.GetUserByIdAccount(account.idAccount);
            if (user == null)
            {
                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
                return response;
            }

            ActiveUser activeUser = new ActiveUser
            {
                Nickname = user.nickname,
                Avatar = user.avatar,
            };
            GuessWhoService.UpdateUserStatus(activeUser, true);

            response.Value = new Profile
            {
                Email = account.email,
                Password = account.password,
                NickName = user.nickname,
                FullName = user.fullName,
                Avatar = user.avatar,
            };
            return response;
        }

        public Response<bool> RegisterUser(Profile profile)
        {
            User user = new User
            {
                nickname = profile.NickName,
                fullName = profile.FullName,
                avatar = profile.Avatar,
            };
            Account account = new Account
            {
                email = profile.Email,
                password = profile.Password,
            };
            
            return UserDAO.RegisterUser(user, account);
        }

        public Response<Profile> VerifyUserRegisteredByEmail(string email)
        {
            Response<Profile> userStored = UserDAO.GetUserByEmail(email);
            return userStored;
        }

        public Response<Profile> VerifyUserRegisteredByNickName(string nickname)
        {
            Response<Profile> userStored = UserDAO.GetUserByNickName(nickname);
            return userStored;
        }

        public void Logout(string nickname)
        {
            ActiveUser activeUser = new ActiveUser
            {
                Nickname = nickname
            };
            GuessWhoService.UpdateUserStatus(activeUser, false);
        }
    }
}
