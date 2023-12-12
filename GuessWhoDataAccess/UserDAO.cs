using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;

namespace GuessWhoDataAccess
{
    public class UserDAO
    {
        public static Response<Account> VerifyUserSession(string email, string password)
        {
            Response<Account> response = new Response<Account>
            {
                StatusCode = ResponseStatus.OK,
                Value = null
            };

            try
            {
                using (var context = new GuessWhoContext())
                {
                    response.Value = context.Accounts.FirstOrDefault(a => a.email == email && a.password == password);
                }
            }
            catch(SqlException)
            {
                response.StatusCode = ResponseStatus.SQL_ERROR;
            }

            return response;
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

        public static Response<User> GetUserByIdAccount(int idAccount)
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
                    response.Value = context.Users.FirstOrDefault(u => u.idAccount == idAccount);
                }
            }
            catch (SqlException)
            {
                response.StatusCode = ResponseStatus.SQL_ERROR;
            }

            return response;
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
                            Password = account.password,
                            IdUser = user.idAccount,
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

        public static Response<bool> UpdateUserProfileImage(byte[] newImage, int idUser)
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
                    var user = context.Users.Find(idUser);

                    if (user != null)
                    {
                        user.avatar = newImage;

                        context.SaveChanges();
                    }
                    else
                    {
                        response.Value = false;
                    }
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

        public static Response<DateTime> GetLastTimeNicknameChangeById(int idUser)
        {
            Response<DateTime> response = new Response<DateTime>
            {
                StatusCode = ResponseStatus.OK,
                Value = DateTime.MinValue,
            };
            try
            {
                using (var context = new GuessWhoContext())
                {
                    var user = context.Users.Find(idUser);

                    if(user != null && user.lastTimeNicknameChanged.HasValue)
                    {
                        response.Value = (DateTime)user.lastTimeNicknameChanged;
                    }
                    else if(user == null)
                    {
                        response.Value = DateTime.MaxValue;
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                response.StatusCode = ResponseStatus.UPDATE_ERROR;
            }
            catch (DbEntityValidationException ex)
            {
                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
            }
            catch (SqlException ex)
            {
                response.StatusCode = ResponseStatus.SQL_ERROR;
            }

            return response;
        } 

        public static Response<bool> UpdateUserNickname(string newNickname, int idUser)
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
                    var user = context.Users.Find(idUser);

                    if(user != null)
                    {
                        user.nickname = newNickname;
                        user.lastTimeNicknameChanged = DateTime.Now;

                        context.SaveChanges();
                    }
                    else
                    {
                        response.Value = false;
                    }
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

        public static Response<bool> UpdateUserFullname(string newFullName, int idUser)
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
                    var user = context.Users.Find(idUser);

                    if (user != null)
                    {
                        user.fullName = newFullName;

                        context.SaveChanges();
                    }
                    else
                    {
                        response.Value = false;
                    }
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

        public static Response<bool> UpdateAccountPassword(string newPassword, int idAccount)
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
                    var account = context.Accounts.Find(idAccount);

                    if (account != null)
                    {
                        account.password = newPassword;

                        context.SaveChanges();
                    }
                    else
                    {
                        response.Value = false;
                    }
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

        public static Response<byte[]> GetUserAvatarByNickname(string userNickname)
        {
            Response<byte[]> response = new Response<byte[]>
            {
                StatusCode = ResponseStatus.VALIDATION_ERROR,
                Value = null
            };

            if(!string.IsNullOrEmpty(userNickname))
            {
                try
                {
                    using (var context = new GuessWhoContext())
                    {
                        var user = context.Users.FirstOrDefault(u => u.nickname == userNickname);
                        if (user != null)
                        {
                            response.Value = user.avatar;
                            response.StatusCode = ResponseStatus.OK;
                        }
                    }
                }
                catch (SqlException)
                {
                    response.StatusCode = ResponseStatus.SQL_ERROR;
                }
            }

            return response;
        }
    }
}
