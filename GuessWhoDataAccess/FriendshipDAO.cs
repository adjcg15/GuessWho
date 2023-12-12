using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;

namespace GuessWhoDataAccess
{
    public class FriendshipDAO
    {
        public static Response<bool> AddRequest(int idUserRequester, int idUserRequested)
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
                    var friendship = new Friendship
                    {
                        idFriendRequester = idUserRequester,
                        idFriendRequested = idUserRequested,
                        status = "Pending"
                    };

                    context.Friendships.Add(friendship);
                    context.SaveChanges();
                }
            }
            catch (DbUpdateException ex) {
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

        public static Response<bool> AcceptRequest(int idFriendship)
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
                    var friendship = context.Friendships.FirstOrDefault(f => f.idFriendship == idFriendship);
                    friendship.status = "Accepted";

                    context.Entry(friendship).State = EntityState.Modified;
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


        public static Response<bool> DeclineRequest(int idFriendship)
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
                    var friendship = context.Friendships.FirstOrDefault(f => f.idFriendship == idFriendship);

                    context.Friendships.Remove(friendship);
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


        public static Response<List<Friendship>> GetRequestsById(int idUserRequested)
        {
            Response<List<Friendship>> response = new Response<List<Friendship>>();
            response.Value = new List<Friendship>();
            response.StatusCode = ResponseStatus.OK;

            try
            {
                using (var context = new GuessWhoContext())
                {
                    var friendships = context.Friendships
                    .Include(f => f.User) 
                    .Include(f => f.User1) 
                    .Where(f => f.User1.idAccount == idUserRequested && f.status == "Pending")
                    .ToList();

                    response.Value = friendships;
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


        public static Response<List<User>> GetFriendsById(int idActualUser)
        {
            Response<List<User>> response = new Response<List<User>>();
            response.Value = new List<User>();
            response.StatusCode = ResponseStatus.OK;

            try
            {
                using(var context = new GuessWhoContext())
                {
                    var friends = context.Friendships
                    .Where(f => (f.idFriendRequester == idActualUser || f.idFriendRequested == idActualUser) && f.status == "Accepted")
                    .Select(f => f.idFriendRequester == idActualUser ? f.User1 : f.User)
                    .Where(u => u.idUser != idActualUser)
                    .ToList();

                    response.Value = friends;
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
    }
}
