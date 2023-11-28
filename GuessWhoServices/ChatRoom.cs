namespace GuessWhoServices
{
    public class ChatRoom
    {
        private IChatCallback firstUserIChatRoomChannel;
        private IChatCallback secondUserInChatRoomChannel;

        public IChatCallback FirstUserInChatRoomChannel { get { return firstUserIChatRoomChannel; } set { firstUserIChatRoomChannel = value; } }

        public IChatCallback SecondUserInChatRoomChannel { get { return secondUserInChatRoomChannel; } set { secondUserInChatRoomChannel = value; } }
    }
}
