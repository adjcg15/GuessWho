using GuessWhoDataAccess;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IAuthenticationService
    {
        public Response<Profile> Login(string email, string password)
        {
            Response<Profile> response = new Response<Profile>()
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
                IdUser = user.idUser
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

        public Response<User> VerifyUserRegisteredByEmail(string email)
        {
            Response<User> userStored = UserDAO.GetUserByEmail(email);
            return userStored;
        }

        public Response<User> VerifyUserRegisteredByNickName(string nickname)
        {
            Response<User> userStored = UserDAO.GetUserByNickName(nickname);
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
