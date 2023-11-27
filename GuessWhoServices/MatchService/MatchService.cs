using GuessWhoDataAccess;
using GuessWhoServices.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Text.RegularExpressions;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IMatchService
    {
        private static Dictionary<string, MatchInformation> matches = new Dictionary<string, MatchInformation>();

        public Dictionary<string, MatchInformation> GetMatches()
        {
            return matches;
        }

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

            Console.WriteLine("Creando la partida " + invitationCode + " para suscriptor " + match.HostChannel.GetHashCode());

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

                //If guest channel stored in match is null it means than no player is in match with the host
                if(storedMatch.GuestChannel == null)
                {
                    response.StatusCode = ResponseStatus.OK;

                    storedMatch.GuestChannel = OperationContext.Current.GetCallbackChannel<IMatchCallback>();
                    storedMatch.GuestNickname = nickname;

                    Console.WriteLine("Suscribiendo a partida " + invitationCode + " al jugador " + storedMatch.GuestChannel.GetHashCode());

                    response.Value = new PlayerInMatch();
                    response.Value.Nickname = "";
                    response.Value.Avatar = null;
                    response.Value.FullName = "";
                    response.Value.IsHost = true;

                    //If HostNickname is empty it is assumed that the host is a guest so it does not have an account
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

                    //If nickname is empty it is assumed that the player is a guest so it does not have an account
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

                    try
                    {
                        storedMatch.HostChannel.PlayerStatusInMatchChanged(guest, true);
                    } 
                    catch(CommunicationObjectAbortedException)
                    {
                        response.StatusCode = ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR;
                        response.Value = null;

                        matches.Remove(invitationCode);
                    }
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

                bool isTheSameClientStoredInMatch = storedGuestChannel != null && (clientChannel.GetHashCode() == storedGuestChannel.GetHashCode());
                if (isTheSameClientStoredInMatch)
                {
                    response.StatusCode = ResponseStatus.OK;
                    response.Value = true;

                    Console.WriteLine("Saliendo de partida " + invitationCode + " el suscriptor " + storedMatch.GuestChannel.GetHashCode());
                    storedMatch.GuestNickname = null;
                    storedMatch.GuestChannel = null;

                    PlayerInMatch emptyPlayer = new PlayerInMatch();
                    emptyPlayer.Nickname = "";
                    emptyPlayer.Avatar = null;
                    emptyPlayer.FullName = "";
                    emptyPlayer.IsHost = false;

                    try
                    {
                        storedMatch.HostChannel.PlayerStatusInMatchChanged(emptyPlayer, false);
                    }
                    catch (CommunicationObjectAbortedException)
                    {
                        Console.WriteLine("Avisando a suscriptor " + storedMatch.HostChannel.GetHashCode() + " de salida de partida " + invitationCode);
                        response.StatusCode = ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR;

                        matches.Remove(invitationCode);
                    }
                }
            }

            return response;
        }

        public Response<bool> FinishGame(string invitationCode)
        {
            var response = new Response<bool>
            {
                StatusCode = ResponseStatus.VALIDATION_ERROR,
                Value = false
            };

            if (matches.ContainsKey(invitationCode))
            {
                var storedMatch = matches[invitationCode];
                var storedHostChannel = storedMatch.HostChannel;
                var clientChannel = OperationContext.Current.GetCallbackChannel<IMatchCallback>();

                bool isClientAllowedToFinishGame = clientChannel.GetHashCode() == storedHostChannel.GetHashCode();
                if (isClientAllowedToFinishGame)
                {
                    Console.WriteLine("Suscriptor " + storedMatch.HostChannel.GetHashCode() + " cancelando partida " + invitationCode);
                    response.StatusCode = ResponseStatus.OK;
                    response.Value = true;

                    PlayerInMatch emptyPlayer = new PlayerInMatch();
                    emptyPlayer.Nickname = "";
                    emptyPlayer.Avatar = null;
                    emptyPlayer.FullName = "";
                    emptyPlayer.IsHost = true;

                    if (storedMatch.GuestChannel != null)
                    {
                        try
                        {
                            Console.WriteLine("Eliminando partida " + invitationCode + " y avisando a suscriptor " + storedMatch.GuestChannel.GetHashCode());
                            storedMatch.GuestChannel.PlayerStatusInMatchChanged(emptyPlayer, false);
                        }
                        catch (CommunicationObjectAbortedException)
                        {
                            response.StatusCode = ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR;
                        }
                    }

                    matches.Remove(invitationCode);
                }
            }

            return response;
        }

        public Response<bool> SendMessage(string invitationCode, string message)
        {
            var response = new Response<bool>
            {
                StatusCode = ResponseStatus.VALIDATION_ERROR,
                Value = false
            };

            if (matches.ContainsKey(invitationCode))
            {
                var storedMatch = matches[invitationCode];
                var senderChannel = OperationContext.Current.GetCallbackChannel<IMatchCallback>();

                response.StatusCode = ResponseStatus.OK;
                response.Value = true;

                try
                {
                    bool isHostSendingMessage = senderChannel.GetHashCode() == storedMatch.HostChannel.GetHashCode();  
                    if (isHostSendingMessage)
                    {
                        if(storedMatch.GuestChannel != null)
                        {
                            storedMatch.GuestChannel.NotifyNewMessage(message, storedMatch.HostNickname);
                        }
                        else
                        {
                            response.Value = false;
                        }
                    }
                    else
                    {
                        storedMatch.HostChannel.NotifyNewMessage(message, storedMatch.GuestNickname);
                    }
                }
                catch (CommunicationObjectAbortedException)
                {
                    response.StatusCode = ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR;
                    response.Value = false;

                    matches.Remove(invitationCode);
                }
            }

            return response;
        }

        public void StartCharacterSelection()
        {
            throw new NotImplementedException();
        }

        public void SelectCharacter(string characterName)
        {
            throw new NotImplementedException();
        }

        public void StartGame(string characterName)
        {
            throw new NotImplementedException();
        }

        public Response<bool> GuessCharacter(string characterName)
        {
            throw new NotImplementedException();
        }

        public void SendClue(bool looksLikeMyCharacter)
        {
            throw new NotImplementedException();
        }
    }
}
