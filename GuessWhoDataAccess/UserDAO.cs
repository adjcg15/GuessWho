using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;

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

        public static Response<bool> RegisterUser(User user, Account account)
        {
            Response<bool> response = new Response<bool>
            {
                StatusCode = ResponseStatus.OK,
                Value = true
            };

            try
            {
                using (var context = new GuessWhoContext())
                {
                    context.Accounts.Add(account);
                    context.SaveChanges();

                    user.idAccount = account.idAccount;
                    context.Users.Add(user);
                    context.SaveChanges();
                }
            } 
            catch (DbUpdateException ex)
            {
                response.StatusCode = ResponseStatus.UPDATE_ERROR;
                response.Value = false;
            }
            catch (DbEntityValidationException ex)
            {
                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
                response.Value = false;
            }
            catch (SqlException ex)
            {
                response.StatusCode = ResponseStatus.SQL_ERROR;
                response.Value = false;
            }

            return response;
        }

        public static User GetUserByIdAccount(int idAccount)
        {
            using(var context = new GuessWhoContext())
            {
                var user = context.Users.FirstOrDefault(u => u.idAccount == idAccount);

                return user;
            }
        }

        public static Response<User> GetUserByEmail(string email)
        {
            Response<User> response = new Response<User>
            {
                StatusCode = ResponseStatus.OK,
                Value = null
            };

            try
            {
                using (var context = new GuessWhoContext())
                {
                    var account = context.Accounts.FirstOrDefault(a => a.email == email);
                    if (account == null)
                    {
                        return null;
                    }

                    var user = context.Users.FirstOrDefault(a => a.idAccount == account.idAccount);
                    response.Value = user;
                }
            }
            catch (SqlException ex)
            {
                response.StatusCode = ResponseStatus.SQL_ERROR;
            }

            return response;
        }

        public static Response<User> GetUserByNickName(string nickname)
        {
            Response<User> response = new Response<User>
            {
                StatusCode = ResponseStatus.OK,
                Value = null
            };

            try
            {
                using (var context = new GuessWhoContext())
                {
                    var user = context.Users.FirstOrDefault(a => a.nickname == nickname);
                    response.Value = user;
                }
            }
            catch (SqlException ex)
            {
                response.StatusCode = ResponseStatus.SQL_ERROR;
            }

            return response;
        }
    }
}
