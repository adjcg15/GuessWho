using GuessWhoDataAccess;
using GuessWhoServices.Utils;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IGameService 
    {
        private static Dictionary<string, MatchInformation> matches = new Dictionary<string, MatchInformation>();

        public Dictionary<string, MatchInformation> GetMatches()
        {
            return matches;
        }

        public Response<string> CreateMatch(string hostNickname, bool isTournamentMatch)
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
            match.HostChannel = OperationContext.Current.GetCallbackChannel<IGameCallback>();
            match.IsTournamentMatch = isTournamentMatch;

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
                
                if(storedMatch.IsTournamentMatch && string.IsNullOrEmpty(nickname))
                {
                    response.StatusCode = ResponseStatus.NOT_ALLOWED;
                }
                else if(storedMatch.GuestChannel == null)
                {
                    response.StatusCode = ResponseStatus.OK;

                    storedMatch.GuestChannel = OperationContext.Current.GetCallbackChannel<IGameCallback>();
                    storedMatch.GuestNickname = nickname;

                    Console.WriteLine("Suscribiendo a partida " + invitationCode + " al jugador " + storedMatch.GuestChannel.GetHashCode());

                    response.Value = new PlayerInMatch();
                    response.Value.Nickname = "";
                    response.Value.Avatar = null;
                    response.Value.FullName = "";

                    //If HostNickname is empty it is assumed that the host is a guest so it does not have an account
                    if (!string.IsNullOrEmpty(storedMatch.HostNickname))
                    {
                        Response<Profile> userResponse = UserDao.GetUserByNickName(storedMatch.HostNickname);

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

                    PlayerInMatch guest = VerifyGuestWithAccount(nickname);

                    try
                    {
                        storedMatch.HostChannel.PlayerStatusInMatchChanged(guest, true);
                    } 
                    catch(CommunicationObjectAbortedException ex)
                    {
                        ServerLogger.Instance.Error(ex.Message);

                        response.StatusCode = ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR;
                        response.Value = null;

                        matches.Remove(invitationCode);
                    }
                    catch (CommunicationException ex)
                    {
                        ServerLogger.Instance.Error(ex.Message);

                        response.StatusCode = ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR;
                        response.Value = null;

                        matches.Remove(invitationCode);
                    }
                }
            }

            return response;
        }

        private PlayerInMatch VerifyGuestWithAccount(string nickname)
        {
            PlayerInMatch guest = new PlayerInMatch
            {
                Nickname = "",
                Avatar = null,
                FullName = ""
            };

            //If nickname is empty it is assumed that the player is a guest so it does not have an account
            if (!string.IsNullOrEmpty(nickname))
            {
                Response<Profile> userResponse = UserDao.GetUserByNickName(nickname);

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

            return guest;
        }

        public void ExitGame(string invitationCode)
        {
            if (matches.ContainsKey(invitationCode))
            {
                var storedMatch = matches[invitationCode];
                var storedGuestChannel = storedMatch.GuestChannel;
                var clientChannel = OperationContext.Current.GetCallbackChannel<IGameCallback>();

                bool isTheSameClientStoredInMatch = storedGuestChannel != null && (clientChannel.GetHashCode() == storedGuestChannel.GetHashCode());
                if (isTheSameClientStoredInMatch)
                {
                    Console.WriteLine("Saliendo de partida " + invitationCode + " el suscriptor " + storedMatch.GuestChannel.GetHashCode());
                    storedMatch.GuestNickname = null;
                    storedMatch.GuestChannel = null;

                    PlayerInMatch emptyPlayer = new PlayerInMatch();
                    emptyPlayer.Nickname = "";
                    emptyPlayer.Avatar = null;
                    emptyPlayer.FullName = "";

                    try
                    {
                        storedMatch.HostChannel.PlayerStatusInMatchChanged(emptyPlayer, false);
                    }
                    catch (CommunicationObjectAbortedException ex)
                    {
                        ServerLogger.Instance.Error(ex.Message);
                        Console.WriteLine("Avisando a suscriptor " + storedMatch.HostChannel.GetHashCode() + " de salida de partida " + invitationCode);
                        
                        matches.Remove(invitationCode);
                    }
                    catch (CommunicationException ex)
                    {
                        ServerLogger.Instance.Error(ex.Message);
                        Console.WriteLine("Avisando a suscriptor " + storedMatch.HostChannel.GetHashCode() + " de salida de partida " + invitationCode);

                        matches.Remove(invitationCode);
                    }
                }
            }
        }

        public void FinishGame(string invitationCode)
        {
            if (matches.ContainsKey(invitationCode))
            {
                var storedMatch = matches[invitationCode];
                var storedHostChannel = storedMatch.HostChannel;
                var clientChannel = OperationContext.Current.GetCallbackChannel<IGameCallback>();

                bool isClientAllowedToFinishGame = clientChannel.GetHashCode() == storedHostChannel.GetHashCode();
                if (isClientAllowedToFinishGame)
                {
                    Console.WriteLine("Suscriptor " + storedMatch.HostChannel.GetHashCode() + " cancelando partida " + invitationCode);

                    PlayerInMatch emptyPlayer = new PlayerInMatch();
                    emptyPlayer.Nickname = "";
                    emptyPlayer.Avatar = null;
                    emptyPlayer.FullName = "";

                    if (storedMatch.GuestChannel != null)
                    {
                        try
                        {
                            Console.WriteLine("Eliminando partida " + invitationCode + " y avisando a suscriptor " + storedMatch.GuestChannel.GetHashCode());
                            storedMatch.GuestChannel.PlayerStatusInMatchChanged(emptyPlayer, false);
                        }
                        catch (CommunicationObjectAbortedException ex)
                        {
                            ServerLogger.Instance.Error(ex.Message);
                        }
                        catch (CommunicationException ex)
                        {
                            ServerLogger.Instance.Error(ex.Message);
                        }
                    }

                    matches.Remove(invitationCode);
                }
            }
        }
    }
}
