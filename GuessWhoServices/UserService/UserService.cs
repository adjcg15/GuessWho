using GuessWhoDataAccess;
using System;

namespace GuessWhoServices
{
    public class UserService : IUserService
    {
        public Profile Login(string email, string password)
        {
            Account account = UserDAO.VerifyUserSession(email, password);

            if(account == null)
            {
                return null;
            }

            User user = UserDAO.GetUserByIdAccount(account.idAccount);

            if(user == null)
            {
                return null;
            }

            return new Profile
            {
                Email = account.email,
                Password = account.password,
                NickName = user.nickname,
                FullName = user.fullName,
                Avatar = user.avatar,
            };

        }

        public bool RegisterUser(Profile profile)
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

        public bool VerifyUserRegisteredByEmail(string email)
        {
            User userStored = UserDAO.GetUserByEmail(email);
            return userStored != null;
        }

        public bool VerifyUserRegisteredByNickName(string nickname)
        {
            User userStored = UserDAO.GetUserByNickName(nickname);
            return userStored != null;
        }
    }
}
