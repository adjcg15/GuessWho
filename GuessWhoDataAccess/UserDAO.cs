﻿using System;
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
                Console.WriteLine(account.email + ", " + account.password);
                context.Accounts.Add(account);
                context.SaveChanges();

                user.idAccount = account.idAccount;
                Console.WriteLine(user.fullName + ", " + user.nickname + ", " + account.idAccount);
                context.Users.Add(user);
                context.SaveChanges();
            }
            return true;
        }
    }
}
