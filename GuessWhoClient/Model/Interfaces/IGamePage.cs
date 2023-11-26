using GuessWhoClient.GameServices;

namespace GuessWhoClient
{
    public interface IGamePage
    {
        void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch);
    }
}
