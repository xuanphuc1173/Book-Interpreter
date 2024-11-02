namespace EXE.ViewModels
{
    public class CreateBlogViewModel
    {
        public int BlogId { get; set; }
        public string Title { get; set; }
        public string? Contents { get; set; }
        public string? Contents2 { get; set; }
        public string? Contents3 { get; set; }
        public IFormFile? Image { get; set; }
        public IFormFile? Image2 { get; set; }
        public IFormFile? Image3 { get; set; }
        public DateTime PublishedDate { get; set; }

    }
}
