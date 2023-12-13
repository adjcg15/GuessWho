namespace GuessWhoServices
{
    public class MatchInformation
    {
        public string HostNickname { get; set ; }
        public string GuestNickname { get; set; }
        public IGameCallback HostChannel { get; set; }
        public IGameCallback GuestChannel { get; set; }
        public bool IsTournamentMatch { get; set; }
    }
}
