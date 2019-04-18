using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExam.Models
{
    public class AddQues
    {
        public int QuestonId { get; set; }
        public int TestId { get; set; }
        public string TestName { get; set; }
        public int QuestionCategoryId { get; set; }
        public string QuestionType { get; set; }
        public string Question { get; set; }
        public decimal Points { get; set; }
        public bool IsActive { get; set; }
        public List<Choice> Choices { get; set; }   
    }
    public class Choices
    {
        public int QuestionId { get; set; }
        public string Label { get; set; }
        public decimal Point { get; set; }
        public bool IsActive { get; set; }
    }
}