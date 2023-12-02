using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GuessWhoServices
{
    public class DrawInformation
    {
        private IDrawServiceCallback playerOneChannel;
        private IDrawServiceCallback playerTwoChannel;

        public IDrawServiceCallback PlayerOneChannel { get { return playerOneChannel; } set { playerOneChannel = value; } }
        public IDrawServiceCallback PlayerTwoChannel { get { return playerTwoChannel; } set { playerTwoChannel = value; } }
    }
}
