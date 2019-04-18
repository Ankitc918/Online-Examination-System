using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExam.Models
{
    public class ChoiceModel
    {
        public int ChoiceId { get; set; }
        public string IsChecked { get; set; }
       

    }

    public class AnswerModel
    {
        public int TestId { get; set; }
        public int QuestionId { get; set; }
        public string Choicesindb { get; set; }
        public Guid Token { get; set; }
        public List<ChoiceModel> UserChoices { get; set; }
        public string Answer { get; set; }
        public string Direction { get; set; }
       
        public List<ChoiceModel> UserSelectedId
        {
            get
            {
               return UserChoices == null ? new List<ChoiceModel>():
                 UserChoices.Where(x => x.IsChecked == "on" || "true".Equals(x.IsChecked, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }
        }
    }
}