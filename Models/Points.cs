namespace EXE.Models
{
    public class Points
    {
        public int PointId { get; set; }
        public int AccountId { get; set; }
        public int TotalPoints {  get; set; }
        public string InvitationCode { get; set; }

        public  Account Account { get; set; }
    }
}
