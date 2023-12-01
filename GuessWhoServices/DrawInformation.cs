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
        private List<SerializedLine> playerOneDraw;
        private List<SerializedLine> playerTwoDraw;
        private IDrawServiceCallback playerOneChannel;
        private IDrawServiceCallback playerTwoChannel;

        public List<SerializedLine> PlayerOneDraw { get { return playerOneDraw; } set { playerOneDraw = value; } }
        public List<SerializedLine> PlayerTwoDraw { get { return playerTwoDraw; } set { playerTwoDraw = value; } }
        public IDrawServiceCallback PlayerOneChannel { get { return playerOneChannel; } set { playerOneChannel = value; } }
        public IDrawServiceCallback PlayerTwoChannel { get { return playerTwoChannel; } set { playerTwoChannel = value; } }
    }
}
