using GuessWhoClient.GameServices;

namespace GuessWhoClient
{
    public class DataStore
    {
        private static Profile profile;
        private static UserServiceClient usersClient;
        private static ChatServiceClient chatsClient;

        public static Profile Profile { get { return profile; } set { profile = value; } }

        public static UserServiceClient UsersClient { get {  return usersClient; } set { usersClient = value; } }

        public static ChatServiceClient ChatsClient { get {  return chatsClient; } set { chatsClient = value; } }
    }
}
