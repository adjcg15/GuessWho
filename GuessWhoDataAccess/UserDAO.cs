using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoDataAccess
{
    public class UserDAO
    {
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
    }
}
