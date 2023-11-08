﻿using GuessWhoDataAccess;
using GuessWhoServices.Utils;
using System.Collections.Generic;
using System.ServiceModel;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IMatchService
    {
        private static Dictionary<string, MatchInformation> matches = new Dictionary<string, MatchInformation>();

        public Response<string> CreateMatch(string hostNickname)
        {
            var response = new Response<string>
            {
                StatusCode = ResponseStatus.OK,
                Value = ""
            };

            string invitationCode;
            do
            {
                invitationCode = Game.GenerateInvitationCode();
            }
            while (matches.ContainsKey(invitationCode));

            var match = new MatchInformation();
            match.HostNickname = hostNickname;
            match.HostChannel = OperationContext.Current.GetCallbackChannel<IMatchCallback>();
            matches[invitationCode] = match;

            response.Value = invitationCode;

            return response;
        }

        public Response<PlayerInMatch> JoinGame(string invitationCode, string nickname)
        {
            var response = new Response<PlayerInMatch>
            {
                StatusCode = ResponseStatus.VALIDATION_ERROR,
                Value = null
            };

            if (matches.ContainsKey(invitationCode))
            {
                var storedMatch = matches[invitationCode];

                if(storedMatch.GuestChannel == null)
                {
                    storedMatch.GuestChannel = OperationContext.Current.GetCallbackChannel<IMatchCallback>();
                    response.StatusCode = ResponseStatus.OK;

                    response.Value = new PlayerInMatch();
                    response.Value.Nickname = "";
                    response.Value.Avatar = null;
                    response.Value.FullName = "";
                    response.Value.IsHost = true;

                    //If HostNickname is empty it is assumed that the user is a guest so it does not have an account
                    if (!string.IsNullOrEmpty(storedMatch.HostNickname))
                    {
                        Response<Profile> userResponse = UserDAO.GetUserByNickName(storedMatch.HostNickname);

                        if (userResponse.StatusCode == ResponseStatus.OK)
                        {
                            response.Value.Nickname = userResponse.Value.NickName;
                            response.Value.Avatar = userResponse.Value.Avatar;
                            response.Value.FullName = userResponse.Value.FullName;
                        }
                        else
                        {
                            response.Value = null;
                            response.StatusCode = userResponse.StatusCode;
                        }
                    }

                    PlayerInMatch guest = new PlayerInMatch();
                    guest.Nickname = "";
                    guest.Avatar = null;
                    guest.FullName = "";
                    guest.IsHost = false;

                    //If nickname is empty it is assumed that the user is a guest so it does not have an account
                    if (!string.IsNullOrEmpty(nickname))
                    {
                        Response<Profile> userResponse = UserDAO.GetUserByNickName(nickname);
                        if (userResponse.StatusCode == ResponseStatus.OK)
                        {
                            guest.Nickname = userResponse.Value.NickName;
                            guest.Avatar = userResponse.Value.Avatar;
                            guest.FullName = userResponse.Value.FullName;
                        }
                        else
                        {
                            //If guest is null, an error occurred when recovering its information
                            guest = null;
                        }
                    }

                    storedMatch.HostChannel.PlayerStatusInMatchChanged(guest, true);
                }
            }

            return response;
        }

        public Response<bool> ExitGame(string invitationCode)
        {
            var response = new Response<bool>
            {
                StatusCode = ResponseStatus.VALIDATION_ERROR,
                Value = false
            };

            if (matches.ContainsKey(invitationCode))
            {
                var storedMatch = matches[invitationCode];
                var storedGuestChannel = storedMatch.GuestChannel;
                var clientChannel = OperationContext.Current.GetCallbackChannel<IMatchCallback>();

                bool isClientStoredInMatch = storedGuestChannel != null && (clientChannel.GetHashCode() == storedGuestChannel.GetHashCode());
                if (isClientStoredInMatch)
                {
                    response.StatusCode = ResponseStatus.OK;
                    response.Value = true;

                    storedMatch.GuestNickname = null;
                    storedMatch.GuestChannel = null;

                    PlayerInMatch emptyPlayer = new PlayerInMatch();
                    emptyPlayer.Nickname = "";
                    emptyPlayer.Avatar = null;
                    emptyPlayer.FullName = "";
                    emptyPlayer.IsHost = false;

                    storedMatch.HostChannel.PlayerStatusInMatchChanged(emptyPlayer, false);
                }
            }

            return response;
        }
    }
}
