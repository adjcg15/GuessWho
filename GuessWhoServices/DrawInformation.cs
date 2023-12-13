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
        public IDrawServiceCallback PlayerOneChannel { get; set; }
        public IDrawServiceCallback PlayerTwoChannel { get ; set ; }
    }
}
