﻿using GuessWhoClient.GameServices;
using System.ServiceModel;

namespace GuessWhoClient
{
    public class DataStore
    {
        private static Profile profile;
        private static UserServiceClient usersClient;

        public static Profile Profile { get { return profile; } set { profile = value; } }
        public static UserServiceClient UsersClient { get {  return usersClient; } set { usersClient = value; } }
    }
}
