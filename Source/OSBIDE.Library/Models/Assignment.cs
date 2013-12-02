﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Models
{
    public class Assignment
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name="Release Date")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [NotMapped]
        [Display(Name = "Release Time")]
        [DataType(DataType.Time)]
        public DateTime ReleaseTime
        {
            get
            {
                return ReleaseDate;
            }
            set
            {
                //first, zero out the release date's time component
                ReleaseDate = DateTime.Parse(ReleaseDate.ToShortDateString());
                ReleaseDate = ReleaseDate.AddHours(value.Hour);
                ReleaseDate = ReleaseDate.AddMinutes(value.Minute);
            }
        }

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [NotMapped]
        [Display(Name = "Due Time")]
        [DataType(DataType.Time)]
        public DateTime DueTime
        {
            get
            {
                return DueDate;
            }
            set
            {
                //first, zero out the release date's time component
                ReleaseDate = DateTime.Parse(ReleaseDate.ToShortDateString());
                ReleaseDate = ReleaseDate.AddHours(value.Hour);
                ReleaseDate = ReleaseDate.AddMinutes(value.Minute);
            }
        }

        public Assignment()
        {
            ReleaseDate = DateTime.Now;
            DueDate = DateTime.Now;
        }
    }
}
