namespace GuessWhoDataAccess
{
    public class PlayerInMatch
    {
        private string nickname;
        private string fullName;
        private byte[] avatar;
        private bool isHost;

        public string Nickname { get { return nickname; } set { nickname = value; } }
        public string FullName { get { return fullName; } set { fullName = value; } }
        public byte[] Avatar { get { return avatar; } set { avatar = value; } }
        public bool IsHost { get { return isHost; } set { isHost = value; } }
    }
}
