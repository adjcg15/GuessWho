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
        private string nicknameReported;
        private string reportComment;
        private int idReportType;

        [DataMember]
        public string NicknameReported { get { return nicknameReported; } set { nicknameReported = value; } }

        [DataMember]
        public string ReportComment { get { return reportComment; } set { reportComment = value; } }

        [DataMember]
        public int IdReportType { get { return idReportType; } set { idReportType = value; } }
    }
}
