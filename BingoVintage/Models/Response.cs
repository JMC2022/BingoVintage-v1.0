namespace BingoVintage.Models
{
    //Model to Verify Registered User , with personal messages.
    public class Response
    {
        public bool Result { get; set; }
        public string? Code { get; set; }
        public string? Message { get; set; }
        public object? Obj { get; set; }
        
    }
}