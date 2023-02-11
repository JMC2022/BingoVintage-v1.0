namespace BingoVintage.Models
{
    public class BingoBallHistory
    {
        public int BingoBallID { get; set; }
        public int UsrID { get; set; }
        public bool Played { get; set; }
        public DateTime DateOfCreation { get; set; }
        public int BallNo { get; set; }
        public bool Active { get; set; }
    }
}