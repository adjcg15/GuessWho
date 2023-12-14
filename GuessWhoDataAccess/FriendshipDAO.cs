using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;

namespace GuessWhoDataAccess
{
    public static class FriendshipDao
    {
        public const string REQUESTED_STATUS = "Request";
        public const string OFFLINE_STATUS = "Offline";
        public const string ONLINE_STATUS = "Online";
        public const string ACCEPT_REQUEST = "Accept";
        public const string DECLINE_REQUEST = "Decline";
        public const string PENDING_REQUEST = "Pending";
        public const string ACCEPTED_REQUEST = "Accepted";

        public static Response<bool> AddRequest(int idUserRequester, int idUserRequested)
        {
            Response<bool> response = new Response<bool>
            {
                StatusCode = ResponseStatus.OK,
                Value = true
            };

            try
            {
                bool existingRequest = FriendshipDao.CheckExistingFriendRequest(idUserRequester, idUserRequested);

                if (existingRequest)
                {
                    response.StatusCode = ResponseStatus.NOT_ALLOWED;
                    response.Value = false;
                }
                else
                {
                    using (var context = new GuessWhoContext())
                    {
                        var friendship = new Friendship
                        {
                            idFriendRequester = idUserRequester,
                            idFriendRequested = idUserRequested,
                            status = PENDING_REQUEST
                        };

                        context.Friendships.Add(friendship);
                        context.SaveChanges();
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.UPDATE_ERROR;
                response.Value = false;
            }
            catch (DbEntityValidationException ex)
            {
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
                response.Value = false;
            }
            catch (EntityException ex)
            {
                ServerLogger.Instance.Fatal(ex.Message);

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

                    if (friendship != null)
                    {
                        friendship.status = ACCEPTED_REQUEST;

                        context.Entry(friendship).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.UPDATE_ERROR;
                response.Value = false;
            }
            catch (DbEntityValidationException ex)
            {
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
                response.Value = false;
            }
            catch (EntityException ex)
            {
                ServerLogger.Instance.Fatal(ex.Message);

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
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.UPDATE_ERROR;
                response.Value = false;
            }
            catch (DbEntityValidationException ex)
            {
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
                response.Value = false;
            }
            catch (EntityException ex)
            {
                ServerLogger.Instance.Fatal(ex.Message);

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
                        .Where(f => f.User1.idUser == idUserRequested && f.status == PENDING_REQUEST)
                        .ToList();
                    
                    response.Value = friendships;
                }
            }
            catch (DbUpdateException ex)
            {
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.UPDATE_ERROR;
            }
            catch (DbEntityValidationException ex)
            {
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
            }
            catch (EntityException ex)
            {
                ServerLogger.Instance.Fatal(ex.Message);

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
                    .Where(f => (f.idFriendRequester == idActualUser || f.idFriendRequested == idActualUser) && f.status == ACCEPTED_REQUEST)
                    .Select(f => f.idFriendRequester == idActualUser ? f.User1 : f.User)
                    .Where(u => u.idUser != idActualUser)
                    .ToList();

                    response.Value = friends;
                }
            }
            catch (DbUpdateException ex)
            {
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.UPDATE_ERROR;
            }
            catch (DbEntityValidationException ex)
            {
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
            }
            catch (EntityException ex)
            {
                ServerLogger.Instance.Fatal(ex.Message);

                response.StatusCode = ResponseStatus.SQL_ERROR;
            }

            return response;
        }

        private static bool CheckExistingFriendRequest(int idUserRequester, int idUserRequested)
        {
            bool requestExists;

            using (var context = new GuessWhoContext())
            {
                var existingRequest = context.Friendships
                    .Any(f =>
                        (f.idFriendRequester == idUserRequester && f.idFriendRequested == idUserRequested && f.status == PENDING_REQUEST) ||
                        (f.idFriendRequester == idUserRequested && f.idFriendRequested == idUserRequester && f.status == PENDING_REQUEST));

                requestExists = existingRequest;
            }

            return requestExists;
        }
    }
}
