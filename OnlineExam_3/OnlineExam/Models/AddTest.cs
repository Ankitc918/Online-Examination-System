﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExam.Models
{
    public class AddTest
    {
        
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public Nullable< bool> IsActive { get; set; }
        [Required]
        public Nullable<System.TimeSpan> DurationInMinute { get; set; }
    }
} 