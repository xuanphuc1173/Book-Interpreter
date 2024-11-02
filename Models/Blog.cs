using Microsoft.EntityFrameworkCore;

namespace EXE.Models
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string Title { get; set;}
        public string? Contents { get; set;}
        public string? Contents2 { get; set;}
        public string? Contents3 { get; set;}
        public string? Image { get; set;}
        public string? Image2 { get; set;}
        public string? Image3 { get; set;}
        public DateTime PublishedDate { get; set;}
    }
}
