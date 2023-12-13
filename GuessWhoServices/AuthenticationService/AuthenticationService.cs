using GuessWhoDataAccess;
using System.Linq;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IAuthenticationService
    {
        public Response<Profile> Login(string email, string password)
        {
            Response<Profile> response = new Response<Profile>()
            {
                StatusCode = ResponseStatus.VALIDATION_ERROR,
                Value = null
            };

            Response<Account> responseAccount = UserDao.VerifyUserSession(email, password);
            Account account = responseAccount.Value;
            if (account != null)
            {
                Response<User> responseUser = UserDao.GetUserByIdAccount(account.idAccount);
                User user = responseUser.Value;
                if (user != null)
                {
                    var nicknameInActiveUsers = activeUsers.Find((u) => u == user.nickname);
                    if (string.IsNullOrEmpty(nicknameInActiveUsers))
                    {
                        UpdateUserStatus(user.nickname, true);

                        response.StatusCode = ResponseStatus.OK;
                        response.Value = new Profile
                        {
                            Email = account.email,
                            Password = account.password,
                            NickName = user.nickname,
                            FullName = user.fullName,
                            Avatar = user.avatar,
                            IdUser = user.idUser
                        };
                    }
                    else
                    {
                        response.StatusCode = ResponseStatus.NOT_ALLOWED;
                    }
                }
                else
                {
                    response.StatusCode = responseUser.StatusCode;
                }
            }
            else
            {
                response.StatusCode = responseAccount.StatusCode;
            }
            
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
            
            return UserDao.RegisterUser(user, account);
        }

        public Response<Profile> VerifyUserRegisteredByEmail(string email)
        {
            Response<Profile> userStored = UserDao.GetUserByEmail(email);
            return userStored;
        }

        public Response<Profile> VerifyUserRegisteredByNickName(string nickname)
        {
            Response<Profile> userStored = UserDao.GetUserByNickName(nickname);
            return userStored;
        }

        public void Logout(string nickname)
        {
            GuessWhoService.UpdateUserStatus(nickname, false);
        }

        public Response<byte[]> GetAvatar(string userNickname)
        {
            return UserDao.GetUserAvatarByNickname(userNickname);
        }
    }
}
