using GuessWhoDataAccess;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IMatchStatusService
    {
        private static readonly Dictionary<string, MatchPlayerInformation> matchPlayersListening = new Dictionary<string, MatchPlayerInformation>();

        public Response<bool> GuessCharacter(string characterName, string matchCode)
        {
            Console.WriteLine("Entrando a GuessCharacter: " + characterName);
            var response = new Response<bool>
            {
                StatusCode = ResponseStatus.VALIDATION_ERROR,
                Value = false
            };

            if (matchPlayersListening.ContainsKey(matchCode))
            {
                Console.WriteLine("Se encontró la partida con código " + matchCode);

                var currentMatchInfo = matchPlayersListening[matchCode];
                var currentChannel = OperationContext.Current.GetCallbackChannel<IMatchStatusCallback>();

                if (currentChannel.GetHashCode() == currentMatchInfo.HostChannel.GetHashCode())
                {
                    response.Value = string.Equals(characterName.Trim(), currentMatchInfo.GuestSelectedCharacterName.Trim(), StringComparison.OrdinalIgnoreCase);
                    response.StatusCode = ResponseStatus.OK;

                    NotifyOtherPlayer(currentMatchInfo.GuestChannel, response.Value ? MatchStatus.GameLost : MatchStatus.GameWon);

                    if (currentMatchInfo.IsTournamentMatch)
                    {
                        AddScorePoints(response.Value ? currentMatchInfo.HostNickname : currentMatchInfo.GuestNickname);
                    }
                }
                else if (currentChannel.GetHashCode() == currentMatchInfo.GuestChannel.GetHashCode())
                {
                    response.Value = string.Equals(characterName.Trim(), currentMatchInfo.GuestSelectedCharacterName.Trim(), StringComparison.OrdinalIgnoreCase);
                    response.StatusCode = ResponseStatus.OK;

                    NotifyOtherPlayer(currentMatchInfo.HostChannel, response.Value ? MatchStatus.GameLost : MatchStatus.GameWon);

                    if (currentMatchInfo.IsTournamentMatch)
                    {
                        AddScorePoints(response.Value ? currentMatchInfo.GuestNickname : currentMatchInfo.HostNickname);
                    }
                }
            }

            return response;
        }


        private void AddScorePoints(string playerNickname)
        {
            MatchDAO.AddScorePoints(playerNickname);
        }

        public void ListenMatchStatus(string matchCode, string nickname)
        {
            var channel = OperationContext.Current.GetCallbackChannel<IMatchStatusCallback>();
            Console.WriteLine(channel.GetHashCode() + " escuchando estado de partida " + matchCode);

            if (!matchPlayersListening.ContainsKey(matchCode))
            {
                matchPlayersListening[matchCode] = new MatchPlayerInformation
                {
                    HostChannel = channel,
                    HostNickname = nickname,
                    IsTournamentMatch = matches[matchCode].IsTournamentMatch
                };
            }
            else
            {
                matchPlayersListening[matchCode].GuestChannel = channel;
                matchPlayersListening[matchCode].GuestNickname = nickname;
            }
        }

        public void SelectCharacter(string characterName, string matchCode)
        {
            if (matchCode != string.Empty)
            {
                var matchPlayersInfo = matchPlayersListening[matchCode];

                if (string.IsNullOrEmpty(matchPlayersInfo.GuestSelectedCharacterName))
                {
                    matchPlayersInfo.GuestSelectedCharacterName = characterName;
                    matchPlayersInfo.HostChannel.MatchStatusChanged(MatchStatus.PlayerReady);
                }
                else
                {
                    Console.WriteLine("El invitado ya ha seleccionado personaje");
                }
            }
            else
            {
                Console.WriteLine("No se encontró canal registrado");
            }
        }

        public void SendAnswer(bool looksLikeMyCharacter, string matchCode)
        {
            if (matchPlayersListening.ContainsKey(matchCode))
            {
                var currentMatchInfo = matchPlayersListening[matchCode];
                var currentChannel = OperationContext.Current.GetCallbackChannel<IMatchStatusCallback>();

                if (currentChannel.GetHashCode() == currentMatchInfo.HostChannel.GetHashCode())
                {
                    NotifyOtherPlayer(currentMatchInfo.GuestChannel, looksLikeMyCharacter ? MatchStatus.LooksLike : MatchStatus.DoesNotLookLike);
                }
                else if (currentChannel.GetHashCode() == currentMatchInfo.GuestChannel.GetHashCode())
                {
                    NotifyOtherPlayer(currentMatchInfo.HostChannel, looksLikeMyCharacter ? MatchStatus.LooksLike : MatchStatus.DoesNotLookLike);
                }
            }
        }

        public void StartCharacterSelection(string matchCode)
        {
            if (matchPlayersListening.ContainsKey(matchCode))
            {
                var currentMatchInfo = matchPlayersListening[matchCode];

                NotifyOtherPlayer(currentMatchInfo.GuestChannel, MatchStatus.CharacterSelection);
            }
            else
            {
                Console.WriteLine("La partida con código " + matchCode + " no existe");
            }
        }


        public void StartGame(string characterName, string matchCode)
        {
            if (matchCode != string.Empty)
            {
                var matchPlayersInfo = matchPlayersListening[matchCode];

                if (string.IsNullOrEmpty(matchPlayersInfo.HostSelectedCharacterName))
                {
                    matchPlayersInfo.HostSelectedCharacterName = characterName;
                    NotifyOtherPlayer(matchPlayersInfo.GuestChannel, MatchStatus.StartGame);
                }
                else
                {
                    Console.WriteLine("El host ya ha seleccionado personaje");
                }
            }
            else
            {
                Console.WriteLine("No se encontró canal registrado");
            }
        }

        public void StopListeningMatchStatus(string matchCode)
        {
            var channel = OperationContext.Current.GetCallbackChannel<IMatchStatusCallback>();
            Console.WriteLine(channel.GetHashCode() + " dejando de escuchar estado de partida " + matchCode);

            if (matchPlayersListening.ContainsKey(matchCode))
            {
                var currentPlayerInfo = matchPlayersListening[matchCode];

                if (currentPlayerInfo.HostChannel == channel)
                {
                    currentPlayerInfo.HostChannel = null;
                }
                else if (currentPlayerInfo.GuestChannel == channel)
                {
                    currentPlayerInfo.GuestChannel = null;
                }

                if (currentPlayerInfo.HostChannel == null && currentPlayerInfo.GuestChannel == null)
                {
                    matchPlayersListening.Remove(matchCode);
                }
            }
        }
        private void NotifyOtherPlayer(IMatchStatusCallback otherPlayerChannel, MatchStatus status)
        {
            if (otherPlayerChannel != null)
            {
                otherPlayerChannel.MatchStatusChanged(status);
            }
        }
    }
}
