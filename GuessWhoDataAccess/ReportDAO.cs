using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoDataAccess
{
    public static class ReportDao
    {
        public static Response<bool> AddPlayerReport(Report playerReport)
        {
            Response<bool> response = new Response<bool>
            {
                StatusCode = ResponseStatus.OK,
                Value = true
            };

            try
            {
                using (var context = new GuessWhoContext())
                {
                    context.Reports.Add(playerReport);
                    context.SaveChanges();
                }
            }
            catch (ArgumentNullException ex)
            {
                ServerLogger.Instance.Warn(ex.Message);

                response.StatusCode = ResponseStatus.UPDATE_ERROR;
                response.Value = false;
            }
            catch (DbUpdateException ex)
            {
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.UPDATE_ERROR;
                response.Value = false;
            }
            catch (DbEntityValidationException ex)
            {
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
                response.Value = false;
            }
            catch (EntityException ex)
            {
                ServerLogger.Instance.Fatal(ex.Message);

                response.StatusCode = ResponseStatus.SQL_ERROR;
                response.Value = false;
            }

            return response;
        }

        public static Response<List<Report>> GetReportsByUserId(int userId)
        {
            Response<List<Report>> response = new Response<List<Report>>
            {
                StatusCode = ResponseStatus.OK,
                Value = new List<Report>()
            };

            try
            {
                using (var context = new GuessWhoContext())
                {
                    response.Value = context.Reports
                    .Where(r => r.idReportedUser == userId)
                    .ToList();
                }
            }
            catch (EntityException ex)
            {
                ServerLogger.Instance.Fatal(ex.Message);

                response.StatusCode = ResponseStatus.SQL_ERROR;
            }

            return response;
        }
    }
}
