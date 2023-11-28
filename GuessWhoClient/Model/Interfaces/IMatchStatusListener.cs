using GuessWhoClient.GameServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoClient.Model.Interfaces
{
    public interface IMatchStatusListener
    {
        void MatchStatusChanged(MatchStatus matchStatusCode);
    }
}
