using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LMS.Models
{
    public class BookCopy
    {
        public BookCopy()
        {
            this.Available = true;
        }

        public int ID { get; set; }
        [Required, Display(Name ="Book")]
        public int BookID { get; set; }
        public virtual Book Book { get; set; }
        [Required]
        public string Location { get; set; }
        [Editable(false)]
        public Boolean Available { get; set; }
        public virtual ICollection<Loan> Loans { get; set; }

        [Editable(false), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? AddedOn { get; set; }
    }
}
