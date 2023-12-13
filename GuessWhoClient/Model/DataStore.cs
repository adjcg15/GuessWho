using GuessWhoClient.GameServices;

namespace GuessWhoClient
{
    public static class DataStore
    {
        public static Profile Profile { get ; set ; }

        public static UserServiceClient UsersClient { get; set; }

        public static ChatServiceClient ChatsClient { get; set; }
    }
}
