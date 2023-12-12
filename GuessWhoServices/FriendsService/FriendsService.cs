﻿using GuessWhoDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IFriendsService
    {
        public Response<bool> SendRequest(int idUserRequester, int idUserRequested)
        {
            return FriendshipDAO.AddRequest(idUserRequester, idUserRequested);
        }

        public Response<List<Friend>> GetRequests(int idUser)
        {
            Response<List<Friend>> response = new Response<List<Friend>>();
            response.Value = new List<Friend>();
            response.StatusCode = ResponseStatus.OK;

            var requestsResponse = FriendshipDAO.GetRequestsById(idUser);
            if (requestsResponse.StatusCode != ResponseStatus.OK)
            {
                response.StatusCode = requestsResponse.StatusCode;
                return response;
            }

            response.Value = requestsResponse.Value
                .Select(friendship => new Friend
                {
                    Nickname = friendship.User.nickname,
                    FullName = friendship.User.fullName,
                    Avatar = friendship.User.avatar,
                    IdFriendship = friendship.idFriendship,
                    Status = FriendshipDAO.REQUESTED_STATUS
                })
                .ToList();

            return response;
        }


        public Response<List<Friend>> GetFriends(int idUser)
        {
            Response<List<Friend>> response = new Response<List<Friend>>();
            response.Value = new List<Friend>();
            response.StatusCode = ResponseStatus.OK;

            var friendsResponse = FriendshipDAO.GetFriendsById(idUser);
            if (friendsResponse.StatusCode != ResponseStatus.OK)
            {
                response.StatusCode = friendsResponse.StatusCode;
                return response;
            }

            var activeUsers = GetActiveUsers();
            var friends = friendsResponse.Value.Select(user =>
            {
                var friend = new Friend
                {
                    Nickname = user.nickname,
                    FullName = user.fullName,
                    Avatar = user.avatar,
                    Status = FriendshipDAO.OFFLINE_STATUS
                };

                if (activeUsers.Any(userNickname => userNickname == user.nickname))
                {
                    friend.Status = FriendshipDAO.ONLINE_STATUS;
                }

                return friend;
            }).ToList();

            response.Value = friends;

            return response;
        }

        public Response<bool> AnswerRequest(int idFriendship, string answer)
        {
            Response<bool> response = new Response<bool>();

            if(answer == FriendshipDAO.ACCEPT_REQUEST)
            {
                response = FriendshipDAO.AcceptRequest(idFriendship);
            }else if(answer == FriendshipDAO.DECLINE_REQUEST)
            {
                response = FriendshipDAO.DeclineRequest(idFriendship);
            }

            return response;
        }
    }
}
