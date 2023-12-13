using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoServices
{
    public class MatchPlayerInformation
    {
        public string HostSelectedCharacterName { get; set; }
        public string GuestSelectedCharacterName { get; set; }
        public string HostNickname { get; set; }
        public string GuestNickname { get; set; }
        public IMatchStatusCallback HostChannel { get; set; }
        public IMatchStatusCallback GuestChannel { get; set; }
        public bool IsTournamentMatch { get; set; }
    }
}
