namespace EXE.ViewModels
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public string InterpreterName { get; set; }
        public decimal TotalCost { get; set; } // TotalCost đã nhân với 1000
                                               // Thêm các thuộc tính khác nếu cần
    }
}
