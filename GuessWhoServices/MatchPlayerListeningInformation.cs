using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoServices
{
    public class MatchPlayerInformation
    {
        private string hostSelectedCharacterName;
        private string guestSelectedCharacterName;
        private IMatchStatusCallback hostChannel;
        private IMatchStatusCallback guestChannel;

        public string HostSelectedCharacterName { get { return hostSelectedCharacterName; } set { hostSelectedCharacterName = value; } }
        public string GuestSelectedCharacterName { get { return guestSelectedCharacterName; } set { guestSelectedCharacterName = value; } }
        public IMatchStatusCallback HostChannel { get { return hostChannel; } set { hostChannel = value; } }
        public IMatchStatusCallback GuestChannel { get { return guestChannel; } set { guestChannel = value; } }
    }
}
