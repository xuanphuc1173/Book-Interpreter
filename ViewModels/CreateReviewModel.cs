using EXE.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EXE.ViewModels
{
    public class CreateReviewModel
    {
        public int ReviewId { get; set; }   
        public int AccountId { get; set;}
        public int InterpreterId { get; set;}
        public string Comment { get; set;}
        public decimal Rating { get; set;}
        public DateTime? ReviewDate { get; set;}
        //public Account Account { get; set; }
        //public Interpreters Interpreter { get; set; }
    }
}
