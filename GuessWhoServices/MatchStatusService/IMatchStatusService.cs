using GuessWhoDataAccess;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract(CallbackContract = typeof(IMatchStatusCallback))]
    public interface IMatchStatusService
    {
        [OperationContract(IsOneWay = true)]
        void ListenMatchStatus(string matchCode);

        [OperationContract(IsOneWay = true)]
        void StartCharacterSelection(string matchCode);

        [OperationContract(IsOneWay = true)]
        void SelectCharacter(string characterName, string matchCode);

        [OperationContract(IsOneWay = true)]
        void StartGame(string characterName, string matchCode);

        [OperationContract]
        Response<bool> GuessCharacter(string characterName, string matchCode);

        [OperationContract(IsOneWay = true)]
        void SendAnswer(bool looksLikeMyCharacter, string matchCode);

        [OperationContract(IsOneWay = true)]
        void StopListeningMatchStatus(string matchCode);
    }

    [ServiceContract]
    public interface IMatchStatusCallback
    {
        [OperationContract]
        void MatchStatusChanged(MatchStatus matchStatusCode);
    }

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
}
