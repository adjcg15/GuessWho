using GuessWhoClient.GameServices;

namespace GuessWhoClient
{
    public class ProfileSingleton
    {
        private static Profile instance;

        public string NickName { get; set; }
        public string FullName { get; set; }
        public byte[] Avatar { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        private ProfileSingleton()
        {
            NickName = string.Empty;
            FullName = string.Empty;
            Avatar = null;
            Email = string.Empty;
            Password = string.Empty;
        }

        public static Profile Instance
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value; 
            }
        }
    }

}
