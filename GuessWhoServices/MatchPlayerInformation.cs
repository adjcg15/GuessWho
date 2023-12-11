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
        private string hostNickname;
        private string guestNickname;
        private IMatchStatusCallback hostChannel;
        private IMatchStatusCallback guestChannel;
        private bool isTournamentMatch;

        public string HostSelectedCharacterName { get { return hostSelectedCharacterName; } set { hostSelectedCharacterName = value; } }
        public string GuestSelectedCharacterName { get { return guestSelectedCharacterName; } set { guestSelectedCharacterName = value; } }
        public string HostNickname { get { return hostNickname; } set { hostNickname = value; } }
        public string GuestNickname { get { return guestNickname; } set { guestNickname = value; } }
        public IMatchStatusCallback HostChannel { get { return hostChannel; } set { hostChannel = value; } }
        public IMatchStatusCallback GuestChannel { get { return guestChannel; } set { guestChannel = value; } }
        public bool IsTournamentMatch { get {  return isTournamentMatch; } set {  isTournamentMatch = value; } }
    }
}
