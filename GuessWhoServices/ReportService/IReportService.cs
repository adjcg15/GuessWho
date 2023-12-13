using GuessWhoDataAccess;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace GuessWhoServices
{
    [ServiceContract]
    public interface IReportService
    {
        [OperationContract]
        Response<bool> ReportPlayer(PlayerReport playerReport);

        [OperationContract]
        Response<List<Report>> GetReportsByUserId(int idUser);

        [OperationContract]
        Response<bool> VerifyPlayerPermanentBanned(string email);

        [OperationContract]
        Response<DateTime> VerifyPlayerTemporarilyBanned(string email);
    }

    [DataContract]
    public class PlayerReport
    {
        [DataMember]
        public string NicknameReported { get; set; }

        [DataMember]
        public string ReportComment { get; set; }

        [DataMember]
        public int IdReportType { get; set; }
    }
}
