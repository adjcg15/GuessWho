namespace GuessWhoServices
{
    public class MatchInformation
    {
        private string hostNickname;
        private string guestNickname;
        private IMatchCallback hostChannel;
        private IMatchCallback guestChannel;

        public string HostNickname { get { return hostNickname; } set { hostNickname = value; } }
        public string GuestNickname { get { return guestNickname; } set { guestNickname = value; } }
        public IMatchCallback HostChannel { get { return hostChannel; } set { hostChannel = value; } }
        public IMatchCallback GuestChannel { get { return guestChannel; } set { guestChannel = value; } }
    }
}
