namespace EXE.Models
{
    public class CalendarAvailability
    {
        public int AvailabilityId { get; set; }
        public int InterpreterId { get; set;}
        public DateOnly AvailabilityDate {  get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Status { get; set; }
    }
}
