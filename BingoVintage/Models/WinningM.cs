namespace BingoVintage.Models
{
    public class WinningM
    {
        public WinningM()
        {
            Numbers = new();
        }

        public int TicketID { get; set; }
        public bool IsLine { get; set; }
        public List<Numbers> Numbers { get; set; }
        public bool IsBingo { get; set; }
    }
}
