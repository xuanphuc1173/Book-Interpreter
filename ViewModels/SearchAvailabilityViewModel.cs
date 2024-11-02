namespace EXE.ViewModels
{
    public class SearchAvailabilityViewModel
    {
        public DateOnly SelectedDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
