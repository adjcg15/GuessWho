using GuessWhoServices;

namespace GuessWhoTests.ServicesTests.MatchStatusService
{
    public class MockMatchStatusClient : IMatchStatusCallback
    {
        public void MatchStatusChanged(MatchStatus matchStatusCode)
        {

        }
    }
}
