using EXE.Models;

namespace EXE.ViewModels
{
    public class AvailabilityViewModel
    {
        public int InterpreterId { get; set; }
        public DateOnly AvailabilityDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Status { get; set; }
        public Interpreters Interpreter { get; set; }

    }
}
