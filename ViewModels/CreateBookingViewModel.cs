namespace EXE.ViewModels
{
    public class CreateBookingViewModel
    {
        public int InterpreterId { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public int AccountId { get; set; }
        public int TotalPoints { get; set; }

    }
}
