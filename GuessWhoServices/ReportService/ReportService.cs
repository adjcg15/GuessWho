using GuessWhoDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoServices
{
    public partial class GuessWhoService : IReportService
    {
        public Response<List<Report>> GetReportsByUserId(int idUser)
        {
            return ReportDAO.GetReportsByUserId(idUser);
        }

        public Response<bool> ReportPlayer(PlayerReport playerReport)
        {
            Response<bool> response = new Response<bool>()
            {
                StatusCode = ResponseStatus.OK,
                Value = false
            };


            Response<Profile> reportedUserProfile = UserDAO.GetUserByNickName(playerReport.NicknameReported);

            if (reportedUserProfile.StatusCode == ResponseStatus.OK && reportedUserProfile.Value != null)
            {
                Report report = new Report()
                {
                    idReportedUser = reportedUserProfile.Value.IdUser,
                    idReportType = playerReport.IdReportType,
                    comment = playerReport.ReportComment,
                    timestamp = DateTime.Now 
                };

                Response<bool> addReportResponse = ReportDAO.AddPlayerReport(report);

                if (addReportResponse.StatusCode == ResponseStatus.OK && addReportResponse.Value)
                {
                    response.Value = true; 
                }
                else
                {
                    response.StatusCode = addReportResponse.StatusCode;
                }
            }
            else
            {
                response.StatusCode = reportedUserProfile.StatusCode;
            }
           
            return response;
        }
    }
}
