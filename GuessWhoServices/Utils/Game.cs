using System.Text;
using System;

namespace GuessWhoServices.Utils
{
    public class Game
    {
        public static string GenerateInvitationCode()
        {
            const int CODE_LENGTH = 8;
            const string CHARACTER_SET = "abcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder invitationCode = new StringBuilder(CODE_LENGTH);
            Random random = new Random();

            for (int i = 0; i < CODE_LENGTH; i++)
            {
                int characterPosition = random.Next(0, CHARACTER_SET.Length);
                invitationCode.Append(CHARACTER_SET[characterPosition]);
            }

            return invitationCode.ToString();
        }
    }
}
