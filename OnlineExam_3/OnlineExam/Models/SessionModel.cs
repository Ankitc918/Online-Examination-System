using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExam.Models
{
    public class SessionModel
    {
        [Required]
        
        public int TestId { get; set; }
        [Required]
        
        public string UserName { get; set; }
        [Display(Name = "Email address")]
        [Required(ErrorMessage = "The email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]  
        public string Email { get; set; }
        //[Required]
        //[DataType(DataType.Password)]
        //[Display(Name = "password")]  
        //public string Password { get; set; }
    }
}