namespace EXE.Models
{
    public class Interpreters
    {
        public int InterpreterId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public decimal Rating { get; set; }
        public int Experience { get; set;}
        public decimal HourlyRate { get; set; }
        public string Location { get; set;}
        public string ContactInfo { get; set; }
        public string Language { get; set; }
        public string Image { get; set; }
        public string Type { get; set; }
        public string Password { get; set; }
        public bool Availability { get; set; }
        public List<Reviews> Reviews { get; set; }
    }
}
