namespace BingoVintage.Models
{
    //Model of Tickets.
    public class Ticket
    {
        public Ticket()
        {
            Numbers = new();
        }

        public int TicketID { get; set; }
        public int UsrID { get; set; }
        public DateTime DateOfCreation { get; set; }
        public int TicketNo { get; set; }
        public List<Numbers> Numbers { get; set; }
        public int Identificator { get; set; }
        public bool Playing { get; set; }
    }
}