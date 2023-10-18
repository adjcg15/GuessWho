using GuessWhoClient.GameServices;
using System.ServiceModel;

namespace GuessWhoClient
{
    public class DataStore
    {
        private static Profile profile;

        private DataStore()
        {
            profile = null;
        }

        public static Profile Profile
        {
            get
            {
                return profile;
            }
            set
            {
                profile = value; 
            }
        }
    }

}
