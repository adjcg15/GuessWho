using GuessWhoDataAccess;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace GuessWhoServices
{
    [DataContract]
    public enum MatchStatus
    {
        [EnumMember]
        CharacterSelection = 0,
        [EnumMember]
        PlayerReady = 1,
        [EnumMember]
        StartGame = 2,
        [EnumMember]
        GameLost = 3,
        [EnumMember]
        GameWon = 4,
        [EnumMember]
        LooksLike = 5,
        [EnumMember]
        DoesNotLookLike = 6
    }

    [ServiceContract(CallbackContract = typeof(IMatchCallback))]
    public interface IMatchService
    {
        [OperationContract]
        Response<string> CreateMatch(string hostNickname);

        [OperationContract]
        Response<PlayerInMatch> JoinGame(string invitationCode, string nickname);

        [OperationContract]
        Response<bool> ExitGame(string invitationCode);

        [OperationContract]
        Response<bool> FinishGame(string invitationCode);

        [OperationContract]
        Response<bool> SendMessage(string invitationCode, string message);

        [OperationContract]
        void StartCharacterSelection();

        [OperationContract]
        void SelectCharacter(string characterName);

        [OperationContract]
        void StartGame(string characterName);

        [OperationContract]
        Response<bool> GuessCharacter(string characterName);

        [OperationContract]
        void SendClue(bool looksLikeMyCharacter);
    }

    [ServiceContract]
    public interface IMatchCallback
    {
        [OperationContract]
        void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch);
        [OperationContract]
        void NotifyNewMessage(string message, string senderNickname);
        [OperationContract]
        void MatchStatusChanged(MatchStatus matchStatus);
    }
}
