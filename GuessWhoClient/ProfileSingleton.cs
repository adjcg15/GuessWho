using GuessWhoClient.GameServices;

namespace GuessWhoClient
{
    public class DataStore
    {
        private static Profile profile;
        private static UserServiceClient usersClient;
        private static MatchServiceClient matchesClient;
        private static string currentMatchCode;
        private static bool isCurrentMatchHost;

        public static Profile Profile { get { return profile; } set { profile = value; } }

        public static UserServiceClient UsersClient { get {  return usersClient; } set { usersClient = value; } }
        
        public static MatchServiceClient MatchesClient { get {  return matchesClient; } set { matchesClient = value; } }

        public static string CurrentMatchCode { get {  return currentMatchCode; } set { currentMatchCode = value; } }
        
        public static bool IsCurrentMatchHost { get {  return isCurrentMatchHost; } set { isCurrentMatchHost = value; } }

        public static void RestartMatchValues()
        {
            isCurrentMatchHost = false;
            currentMatchCode = "";
            matchesClient = null;
        }
    }
}
