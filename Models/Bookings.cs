using EXE.Controllers;
using System.Numerics;

namespace EXE.Models
{
    public class Bookings
    {
        public int BookingId { get; set; }
        public int AccountId { get; set; }
        public int InterpreterId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalCost { get; set; }
        public string Location { get; set; }
        public Account Account { get; set; }
        public Interpreters Interpreter { get; set; }
        public bool HasReview { get; set; }

    }
}
