using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GuessWhoServices
{
    [ServiceContract (CallbackContract = typeof(IDrawServiceCallback))]
    interface IDrawService
    {
        [OperationContract]
        void SendDraw(List<Line> localDrawMap);
    }

    public interface IDrawServiceCallback
    {
        void DrawReceived(List<Line> adversaryDrawMap);
    }
}
