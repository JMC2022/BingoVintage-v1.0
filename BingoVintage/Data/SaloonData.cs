using BingoVintage.Helpers;
using BingoVintage.Models;
using Dapper;
using Microsoft.SqlServer.Server;
using System.Data;
using System.Data.SqlClient;

namespace BingoVintage.Data
{
    public class SaloonData
    {
        private readonly string _cnn;
        public SaloonData()
        {
            var getConection = new GetConnectionString();
            _cnn = getConection.ConnectionString("BingoVintage");
        }

        public void DeleteTmpTicket()
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var delete = "DELETE FROM [BingoVintage].[dbo].[Tmp_Ticket]";
            cnn.Query(delete);
        }

        public void CreateTmpTicket(Ticket t)
        {
            int TmpId = 0;
            List<SqlDataRecord> tickets = new TVP_Helper(TmpId).GetListT(t);
            List<SqlDataRecord> numbers = new TVP_Helper(++TmpId).GetListN(t);

            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            using SqlCommand cmd = cnn.CreateCommand();
            {
                // Define the procedure call.
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.Load_Tmp_Tickets";

                // Add a table parameter for the tickets. First type and direction. Structured is the general type for TVPs.
                cmd.Parameters.Add("@Tmp_Ticket", SqlDbType.Structured);
                cmd.Parameters["@Tmp_Ticket"].Direction = ParameterDirection.Input;

                // The name of the table type.
                cmd.Parameters["@Tmp_Ticket"].TypeName = "dbo.Tickets_tbltype";

                // And the value actually we pass, the Ticket.
                cmd.Parameters["@Tmp_Ticket"].Value = tickets;

                // The same thing for the @Numbers parameter.
                cmd.Parameters.Add("@Tmp_Numbers", SqlDbType.Structured);
                cmd.Parameters["@Tmp_Numbers"].Direction = ParameterDirection.Input;
                cmd.Parameters["@Tmp_Numbers"].TypeName = "dbo.Numbers_tbltype";
                cmd.Parameters["@Tmp_Numbers"].Value = numbers;

                // Run the procedure.
                cmd.ExecuteNonQuery();
            }
        }

        public void LoadTicketToDb()
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var sql = "DECLARE @idmap TABLE (TempID int NOT NULL PRIMARY KEY,\r\n\t\t\t\t\t  TicketID int NOT NULL UNIQUE)\r\n\r\nMERGE Ticket Tk\r\nUSING Tmp_Ticket Tmp ON 1 = 0\r\nWHEN NOT MATCHED BY TARGET THEN\r\n  INSERT (UsrID, TicketNo, Identificator, Playing) \r\n\t\tVALUES(Tmp.UsrID, Tmp.TicketNo, Tmp.Identificator , 1)\r\nOUTPUT Tmp.TicketID , inserted.TicketID INTO @idmap (TempID, TicketId);\r\nINSERT Numbers (TicketID, NumID, Num, Acert)\r\n\tSELECT i.TicketID, N.NumId, N.Num, N.Acert FROM Tmp_Numbers N\r\n\tJOIN @idmap i ON i.TempID = N.TicketID";
            cnn.Execute(sql);
        }

        public List<Ticket> GetActiveListOfTickets(int usrId)
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var sql = "SELECT * FROM Ticket WHERE UsrID = @usrId and Playing = 1";
            List<Ticket> ListT = new();
            using (var multi = cnn.QueryMultiple(sql, new { usrId }))
            {
                ListT = multi.Read<Ticket>().AsList();
            }
            return ListT;
        }
        
        public List<Numbers> GetActiveListOfNumbers(int ticketID)
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var sql = "SELECT * FROM Numbers WHERE TicketID = @ticketID";
            List<Numbers> ListN = new();
            using (var multi = cnn.QueryMultiple(sql, new { ticketID }))
            {
                ListN = multi.Read<Numbers>().AsList();
            }
            return ListN;
        }

        public void DisablePlayingTickets(int usrId)
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            //var sql = "UPDATE Ticket\r\nSET Playing = 0 where UsrID = @usrId and  Playing = 1";
            try
            {
                var sql = "DECLARE @ids TABLE (id int);\r\nBEGIN TRANSACTION\r\n\r\nUPDATE Ticket\r\nSET Playing = 0\r\nOUTPUT INSERTED.UsrID INTO @ids\r\nWHERE Ticket.Playing = 1 and UsrID= @usrId;\r\n\r\nUPDATE BingoBallHistoy \r\nSET Active = 0\r\nFROM BingoBallHistoy\r\nJOIN @ids i on i.id = BingoBallHistoy.UsrID;\r\n\r\nCOMMIT;";
                cnn.Query(sql, new { usrId });
            }
            catch (Exception)
            {
                Console.WriteLine("Algo no salio bien");
                throw;
            }
        }

        public void CreateHistoryForUsr(int num, int usrId)
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var sql = "INSERT INTO dbo.BingoBallHistoy (UsrID, Played, BallNo, Active)\r\nVALUES (@usrId,1,@num,1)";
            cnn.Query(sql, new {usrId , num});
        }

        public int[] GetBallsPlaying()
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var sql = "SELECT BallNo FROM dbo.BingoBallHistoy\r\n  where Played = 1 and Active =1";
            var balls = cnn.Query<int>(sql).ToArray();
            return balls;
        }

        public int GetLastBallPlayed(int usrId)
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var sql = "SELECT BallNo FROM dbo.BingoBallHistoy\r\n  where Played = 1 and Active=1 and UsrID = @usrId order by DateOfCreation desc";
            var lastBall = cnn.QueryFirstOrDefault<int>(sql , new {usrId});
            return lastBall;
        }

        //Get the Number of Playing Tickets.
        public int GetCantPlayingTicket(int usrId)
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var sql = "SELECT * FROM [dbo].[Ticket] WHERE Playing = 1 and UsrID = @usrId;";
            var cantT = cnn.Query<int>(sql , new {usrId}).Count();
            return cantT;
        }

        public bool IsAcert(int UsrId,int num)
        {
            if (num == 0)
            {
                return false;
            }
            else
            {
                using var cnn = new SqlConnection(_cnn);
                cnn.Open();
                var sql = "UPDATE n\r\n  SET n.Acert = 1\r\n  FROM dbo.Numbers as n\r\n  INNER JOIN Ticket as t on t.TicketID = n.TicketID\r\n  WHERE t.Playing = 1 and t.UsrID = @UsrId and n.Num = @num";
                var cantT = cnn.Execute(sql, new { UsrId, num });
                if (cantT != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public List<DataResponseAcert> GetPropertiesOfAcert(int usrId, int num)
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var sql = "SELECT t.TicketNo, n.NumID , n.Acert, n.Num FROM Numbers n\r\nINNER JOIN Ticket t ON t.TicketID = n.TicketID\r\nWHERE t.Playing=1 AND t.UsrID = @usrId AND n.Num = @num";
            List<DataResponseAcert> response = new();
            response = cnn.Query<DataResponseAcert>(sql, new { usrId, num }).ToList();
            return response;
        }

        //Return all acert For Current active User
        public int[] AllAcertForCurrentTicket(int usrId, int ticketId)
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var sql = "SELECT n.NumID\r\nFROM Ticket tk\r\nINNER JOIN Numbers n on n.TicketID = tk.TicketID\r\nWHERE UsrID = @usrId and Playing = 1 and n.Acert = 1 and tk.TicketID = @ticketId";
            var listawinning = cnn.Query<int>(sql, new { usrId, ticketId }).ToArray();
            return listawinning;

        }
        public void CreateTHistory(int TicketID, int Identificator, int UsrID, int TicketNo, bool Line, bool Bingo)
        {
            int BallOfAcert= GetLastBallPlayed(UsrID);
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var sql = "INSERT INTO dbo.TicketHistory (TicketID,Identificator,UsrID,TicketNo,BallOfAcert, Line, Bingo)\r\nVALUES (@TicketID,@Identificator,@UsrID,@TicketNo,@BallOfAcert,@Line,@Bingo)";
            cnn.Query(sql, new { TicketID, UsrID,Identificator,TicketNo,BallOfAcert,Line,Bingo});
        }
        public List<TicketHistory> GetTicketHistories(int UsrId)
        {
            using var cnn = new SqlConnection(_cnn);
            cnn.Open();
            var sql = "Select * From TicketHistory \r\n  Where UsrID = @UsrId";
            List<TicketHistory> histories = new();
            histories = cnn.Query<TicketHistory>(sql, new { UsrId }).ToList();
            return histories;
        }
    }
}