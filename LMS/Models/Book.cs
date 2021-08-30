using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LMS.Models
{
    public class Book
    {
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required, Display(Name="Publisher")]
        public int PublisherID { get; set; }
        public virtual Publisher Publisher { get; set; }
        [Required, Display(Name = "Press")]
        public int PressID { get; set; }
        public virtual Press Press { get; set; }
        [Required, DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? PublishedDate { get; set; }
        [Required]
        public double Charge { get; set; }
        [Required]
        public double PenaltyCharge { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<Author> Authors { get; set; }
        public virtual ICollection<BookCopy> Copies { get; set; }
    }
}