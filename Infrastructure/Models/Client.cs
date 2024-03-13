﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class Client
    {
        [Key]
        public string? AppUserId { get; set; }

        [Display(Name = "W Number")]
        public string? StudentId { get; set; }

        public int? MajorId { get; set; }

        public string? ClassLevel { get; set; }

        [ForeignKey("MajorId")]
        public Major? Major { get; set; }

        [ForeignKey("AppUserId")]
        public AppUser? AppUser { get; set; }
    }
}