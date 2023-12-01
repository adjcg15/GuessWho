using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace GuessWhoServices
{
    [ServiceContract (CallbackContract = typeof(IDrawServiceCallback))]
    interface IDrawService
    {
        [OperationContract(IsOneWay = true)]
        void SubscribeToDrawService(string matchCode);

        [OperationContract(IsOneWay = true)]
        void UnsubscribeFromDrawService(string matchCode);

        [OperationContract(IsOneWay = true)]
        void SendDraw(List<SerializedLine> localDrawMap, string matchCode);
    }

    [ServiceContract]
    public interface IDrawServiceCallback
    {
        [OperationContract]
        void DrawReceived(List<SerializedLine> adversaryDrawMap);
    }

    [DataContract]
    public class SerializedLine
    {
        [DataMember]
        public string Color { get; set; }

        [DataMember]
        public Point StartPoint { get; set; }

        [DataMember]
        public Point EndPoint { get; set; }
    }
}
