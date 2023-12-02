using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Windows;

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
        [OperationContract(IsOneWay = true)]
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
