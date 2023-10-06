using GuessWhoDataAccess;
using System;

namespace GuessWhoServices
{
    public class UserService : IUserService
    {
        public Profile Login(string email, string password)
        {
            throw new NotImplementedException();
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
    }
}
