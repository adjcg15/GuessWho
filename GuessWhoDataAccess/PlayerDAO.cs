using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;

namespace GuessWhoDataAccess
{
    public static class PlayerDao
    {
        public static Response<List<TopPlayer>> GetTopPlayers(string query, int numberOfPlayers)
        {
            Response<List<TopPlayer>> response = new Response<List<TopPlayer>>();
            response.Value = new List<TopPlayer>();
            response.StatusCode = ResponseStatus.OK;

            try
            {
                using (var context = new GuessWhoContext())
                {
                    var result = context.Matches
                        .Join(
                            context.Users,
                            match => match.idWinner,
                            user => user.idUser,
                            (match, user) => new
                            {
                                UserId = user.idUser,
                                UserNickname = user.nickname,
                                MatchScore = match.score,
                            }
                        )
                        .Where(u => u.UserNickname.Contains(query))
                        .GroupBy(m => m.UserId)
                        .Select(g => new
                        {
                            UserNickname = g.FirstOrDefault().UserNickname,
                            TotalScore = g.Sum(m => m.MatchScore)
                        })
                        .OrderByDescending(tp => tp.TotalScore)
                        .Take(numberOfPlayers)
                        .ToList();

                    int position = 1;
                    foreach (var row in result)
                    {
                        if (row != null && row.TotalScore.HasValue)
                        {
                            response.Value.Add(new TopPlayer
                            {
                                Nickname = row.UserNickname,
                                Score = (int)row.TotalScore,
                                Position = position
                            });
                        }
                        position++;
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.UPDATE_ERROR;
            }
            catch (DbEntityValidationException ex)
            {
                ServerLogger.Instance.Error(ex.Message);

                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
            }
            catch (SqlException ex)
            {
                ServerLogger.Instance.Fatal(ex.Message);

                response.StatusCode = ResponseStatus.SQL_ERROR;
            }

            return response;
        }

        public static Response<bool> CheckPlayerPermanentBan(string email)
        {
            string WORST_PLAYER_BEHAVIOUR_REPORT_NAME = "Cheating Player";
            int MAX_ALLOWED_REPORTS = 3;
            Response<bool> response = new Response<bool>
            {
                Value = false,
                StatusCode = ResponseStatus.VALIDATION_ERROR
            };

            try
            {
                using (var context = new GuessWhoContext())
                {
                    var reportType = context.ReportTypes.FirstOrDefault(type => type.name == WORST_PLAYER_BEHAVIOUR_REPORT_NAME);
                    var userAccount = context.Accounts.FirstOrDefault(account => account.email == email);

                    if (reportType != null && userAccount != null)
                    {
                        var userReported = context.Users.FirstOrDefault(user => user.idAccount == userAccount.idAccount);

                        if (userReported != null)
                        {
                            var totalReports = context.Reports.Count(r => r.idReportType == reportType.idReportType && r.idReportedUser == userReported.idUser);

                            response.Value = totalReports >= MAX_ALLOWED_REPORTS;
                            response.StatusCode = ResponseStatus.OK;
                        }
                    }
                }
            }
            catch(SqlException ex)
            {
                ServerLogger.Instance.Fatal(ex.Message);

                response.StatusCode = ResponseStatus.SQL_ERROR;
            }

            return response;
        }

        public static Response<DateTime> CheckPlayerTemporalBan(string email)
        {
            int MAX_MONTHLY_REPORTS = 3;
            int MONTH_DAYS = 30;
            Response<DateTime> response = new Response<DateTime>
            {
                Value = DateTime.MinValue,
                StatusCode = ResponseStatus.VALIDATION_ERROR
            };

            try
            {
                using (var context = new GuessWhoContext())
                {
                    var userAccount = context.Accounts.FirstOrDefault(account => account.email == email);

                    if (userAccount != null)
                    {
                        var userReported = context.Users.FirstOrDefault(user => user.idAccount == userAccount.idAccount);
                        DateTime dateUpperLimit = DateTime.Now;
                        DateTime dateLowerLimit = dateUpperLimit.AddDays(-MONTH_DAYS);

                        if(userReported != null)
                        {
                            var lastPlayerReports = context.Reports
                                .Where(r => r.idReportedUser == userReported.idUser && dateLowerLimit <= r.timestamp && r.timestamp <= dateUpperLimit)
                                .OrderByDescending(r => r.timestamp)
                                .Take(3)
                                .ToList();

                            if (lastPlayerReports.Count >= MAX_MONTHLY_REPORTS)
                            {
                                response.StatusCode = ResponseStatus.NOT_ALLOWED;
                                var dateLastReport = lastPlayerReports[0].timestamp;

                                if (dateLastReport.HasValue)
                                {
                                    response.Value = dateLastReport.Value.AddDays(MONTH_DAYS);
                                }
                            }
                            else
                            {
                                response.StatusCode = ResponseStatus.OK;
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ServerLogger.Instance.Fatal(ex.Message);

                response.StatusCode = ResponseStatus.SQL_ERROR;
            }

            return response;
        }
    }

    [DataContract]
    public class TopPlayer
    {
        [DataMember]
        public string Nickname { get; set; }

        [DataMember]
        public int Score { get; set; }

        [DataMember]
        public int Position { get; set; }
    }
}
