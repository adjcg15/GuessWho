using GuessWhoServices;
using GuessWhoTests.GameServices;

namespace GuessWhoTests.ServicesTests.MatchStatusService
{
    public class MockMatchStatusClient : IMatchStatusServiceCallback
    {
        public bool RedirectedToCharacterSelection { get; private set; }

        public void MatchStatusChanged(MatchStatus matchStatusCode)
        {
            throw new System.NotImplementedException();
        }
    }
}
