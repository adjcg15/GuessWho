using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoDataAccess
{
    public class UserDAO
    {
        public static Account VerifyUserSession(string email, string password)
        {
            using (var context = new GuessWhoContext())
            {
                var account = context.Accounts.FirstOrDefault(a => a.email == email && a.password == password);

                return account;
            }
        }

        public static bool RegisterUser(User user, Account account)
        {
            using (var context = new GuessWhoContext())
            {
                context.Accounts.Add(account);
                context.SaveChanges();

                user.idUser = account.idAccount;
                context.Users.Add(user);
                context.SaveChanges();
            }
            return true;
        }

        public static User GetUserByIdAccount(int idAccount)
        {
            using(var context = new GuessWhoContext())
            {
                var user = context.Users.FirstOrDefault(u => u.idAccount == idAccount);

                return user;
            }
        }
    }
}
