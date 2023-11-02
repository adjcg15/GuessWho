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

        public static Response<Profile> GetUserByEmail(string email)
        {
            Response<Profile> response = new Response<Profile>
            {
                StatusCode = ResponseStatus.OK,
                Value = null
            };

            try
            {
                using (var context = new GuessWhoContext())
                {
                    var account = context.Accounts.FirstOrDefault(a => a.email == email);
                    if (account != null)
                    {
                        var user = context.Users.FirstOrDefault(a => a.idAccount == account.idAccount);

                        response.Value = new Profile
                        {
                            NickName = user.nickname,
                            FullName = user.fullName,
                            Avatar = user.avatar,
                            Email = account.email,
                            Password = account.password
                        };
                    }
                }
            }
            catch (SqlException ex)
            {
                response.StatusCode = ResponseStatus.SQL_ERROR;
            }

            return response;
        }

        public static Response<Profile> GetUserByNickName(string nickname)
        {
            Response<Profile> response = new Response<Profile>
            {
                StatusCode = ResponseStatus.OK,
                Value = null
            };

            try
            {
                using (var context = new GuessWhoContext())
                {
                    var user = context.Users.FirstOrDefault(a => a.nickname == nickname);
                    if (user != null)
                    {
                        var account = context.Accounts.FirstOrDefault(a => a.idAccount == user.idAccount);

                        response.Value = new Profile
                        {
                            NickName = user.nickname,
                            FullName = user.fullName,
                            Avatar = user.avatar,
                            Email = account.email,
                            Password = account.password
                        };
                    }
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
