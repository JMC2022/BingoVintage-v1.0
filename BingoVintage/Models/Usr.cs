namespace BingoVintage.Models
{
    //Model for Usr.
    public class Usr
    {
        private DateTime _lastDayOnline = DateTime.Now;
      
        public int UsrID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public bool PlusEighteen { get; set; }
        public bool Verified { get; set; }
        public DateTime DateOfCreation { get; set; }
        public DateTime LastDayOnline { get { return _lastDayOnline; } set { _lastDayOnline = value; } }
        public bool RememberLogin { get; set; }
    }
}