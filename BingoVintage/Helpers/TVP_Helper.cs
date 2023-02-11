using BingoVintage.Models;
using Microsoft.SqlServer.Server;
using System.Data;
using System.Data.SqlClient;

namespace BingoVintage.Helpers
{
    public class TVP_Helper
    {
        private int _TempID;
        public TVP_Helper(int TempID)
        {
            _TempID = TempID;
        }

        public SqlMetaData[] TTable()
        {
            // Definition for the tickets table type.
            SqlMetaData[] tickets_tbltype =
                 { // TempID. Note that this is defined as unique.
             // This is important to improve performance.
             new SqlMetaData("id", SqlDbType.Int, false,
                            true, SortOrder.Ascending, 0),
             // The other columns.
             new SqlMetaData("UsrID", SqlDbType.Int),
             new SqlMetaData("TicketNo", SqlDbType.Int),
             new SqlMetaData("Identificator", SqlDbType.Int)
            };
            return tickets_tbltype;
        }

        public SqlMetaData[] NTable()
        {
            // And this is the Numbers table type.
            SqlMetaData[] numbers_tbltype =
                    { // In this type, there is a two-column key.
            new SqlMetaData("id", SqlDbType.Int, false,
                            true, SortOrder.Ascending, 0),
            new SqlMetaData("NumID", SqlDbType.TinyInt,  false,
                            true, SortOrder.Ascending, 1),
            // The other columns.
            new SqlMetaData("Num", SqlDbType.Int),
            new SqlMetaData("Acert", SqlDbType.Bit)
            };
            return numbers_tbltype;
        }

        public List<SqlDataRecord> GetListT (Ticket t) 
        {
            // Create a data record for a new Ticket.
            List<SqlDataRecord> tickets = new();
            var t_table = TTable();
            SqlDataRecord ticket_rec = new(t_table);
            // Write the fields. First the id.
            ticket_rec.SetInt32(0, ++_TempID);
            // TicketNo and Identificator.
            ticket_rec.SetInt32(1, t.UsrID);
            ticket_rec.SetInt32(2, t.TicketNo);
            ticket_rec.SetInt32(3, t.Identificator);
            // Add the Tickets to the output list for tickets.
            tickets.Add(ticket_rec);

            return tickets;
        }
        public List<SqlDataRecord> GetListN(Ticket t)
        {
            // Create a data record for numbers.
            List<SqlDataRecord> numbers = new();
            var n_table = NTable();
            byte numId = 0;
            foreach (var number in t.Numbers)
            {
                SqlDataRecord number_rec = new(n_table);
                number_rec.SetInt32(0, _TempID);
                number_rec.SetByte(1, Convert.ToByte(++numId));
                number_rec.SetInt32(2, number.Num);
                number_rec.SetBoolean(3, false);
                numbers.Add(number_rec);
            }
            return numbers;
        }
    }
}