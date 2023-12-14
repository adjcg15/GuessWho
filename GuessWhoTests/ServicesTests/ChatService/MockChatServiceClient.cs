using GuessWhoTests.GameServices;

namespace GuessWhoTests.ServicesTests.ChatService
{
    public class MockChatServiceClient : IChatServiceCallback
    {
        public int TotalMessageReceived { get; private set; }

        public string LatestMessage { get; private set; }

        public void NewMessageReceived(string message)
        {
            TotalMessageReceived++;
            LatestMessage = message;
        }
    }
}
