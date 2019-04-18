using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExam.Models
{
    public class ResultModel
    {
        public int TestId { get; set; }

        public Guid Token { get; set; }

        public string TestName { get; set; }
       
        public string UserName { get; set; }
        

        public Nullable<decimal> MarkScored { get; set; }
    }
}