using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExam.Models
{
    public class  QuestionModel
    {
        [Required]
        public int TotalQuestionInSet { get; set; }
        [Required]
        public int QuestionNumber { get; set; }
        [Required]
        public int TestId { get; set; }
        [Required]
        public string TestName { get; set; }
        [Required]
        public string Question { get; set; }
        [Required]
        public string QuestionType { get; set; }
        [Required]
        public int Point { get; set; }
        [Required]
        public List<QXOModel> Options { get; set; }
        public List<QuesList> Queslist { get; set; }
    }

    public class QuesList 
    {
        public int quesno { get; set; }
        public IQueryable<QXOModel> QXOModel { get; set; }
    }
    public class QXOModel
    {
        public decimal Points { get; set; }
        public bool IsActive { get; set; }
        public int ChoiceId { get; set; }
        public string Label { get; set; }
        public string Answer { get; set; }
    }
}