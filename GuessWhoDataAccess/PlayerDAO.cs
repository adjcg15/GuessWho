using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;

namespace GuessWhoDataAccess
{
    public class PlayerDAO
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
                response.StatusCode = ResponseStatus.UPDATE_ERROR;
            }
            catch (DbEntityValidationException ex)
            {
                response.StatusCode = ResponseStatus.VALIDATION_ERROR;
            }
            catch (SqlException ex)
            {
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
