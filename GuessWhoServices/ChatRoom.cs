namespace GuessWhoServices
{
    public class ChatRoom
    {
        private IChatCallback hostChannel;
        private IChatCallback guestChannel;

        public IChatCallback HostChannel { get { return hostChannel; } set { hostChannel = value; } }

        public IChatCallback GuestChannel { get { return guestChannel; } set { guestChannel = value; } }
    }
}
