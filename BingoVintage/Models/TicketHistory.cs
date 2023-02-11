namespace BingoVintage.Models
{
    public class TicketHistory
    {
        public int THistoryID { get; set; }
        public int TicketID { get; set; }
        public int Identificator { get; set; }
        public int UsrID { get; set; }
        public int TicketNo { get; set; }
        public int BallOfAcert { get; set; }
        public DateTime DateOfCreation { get; set; }
        public bool Line { get; set; }
        public bool Bingo { get; set; }
    }
}
