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

            Response<Account> responseAccount = UserDAO.VerifyUserSession(email, password);
            Account account = responseAccount.Value;
            if (account != null)
            {
                Response<User> responseUser = UserDAO.GetUserByIdAccount(account.idAccount);
                User user = responseUser.Value;
                if (user != null)
                {
                    var nicknameInActiveUsers = activeUsers.FirstOrDefault((u) => u == user.nickname);
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
            GuessWhoService.UpdateUserStatus(nickname, false);
        }

        public Response<byte[]> GetAvatar(string userNickname)
        {
            return UserDAO.GetUserAvatarByNickname(userNickname);
        }
    }
}
