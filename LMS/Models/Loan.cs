using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LMS.Models
{
    public class Loan : IValidatableObject
    {
        public int ID { get; set; }

        [Required, Display(Name="Book Copy")]
        public int BookCopyID { get; set; }
        public virtual BookCopy BookCopy { get; set; }

        [Required, Display(Name = "Member")]
        public int MemberID { get; set; }
        public virtual Member Member { get; set; }

        [Editable(false), DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime IssuedOn { get; set; }

        [DataType(DataType.Date), Editable(false), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ReturnedOn { get; set; }

        [Editable(false)]
        public double LoanCharge { get; set; }

        [Editable(false)]
        public double? PenaltyCharge { get; set; }

        [DataType(DataType.Date), Editable(false), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DueDate { get; set; }

        [Required, Display(Name ="Loan Type")]

        public int LoanTypeID { get; set; }
        public virtual LoanType LoanType { get; set; }

        [Editable(false)]
        public string LoanedByID { get; set; }
        public virtual ApplicationUser LoanedBy { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            using (var db = new ApplicationDbContext())
            {
                if (db.LoanTypes.FirstOrDefault(l => l.ID == LoanTypeID) == null)
                {
                    yield return new ValidationResult("Please select a valid loan type.", new string[] { "LoanTypeID" });
                }

                var bookCopy = db.BookCopies.FirstOrDefault(bc => bc.ID == BookCopyID);

                if (bookCopy == null)
                {
                    yield return new ValidationResult("The book copy is not valid.", new string[] {});
                }

                var member = db.Members.FirstOrDefault(m => m.ID == MemberID);

                if (member == null)
                {
                    yield return new ValidationResult("Please select a valid loan type.", new string[] { "LoanTypeID" });
                }
                else if (bookCopy != null && DateTime.Now.AddYears(-18).CompareTo(member.DateOfBirth) == -1 && bookCopy.Book.Categories.Any(c => c.AgeRestricted == true))
                {
                    yield return new ValidationResult("The member is under age for this book.", new string[] { "MemberID" });
                }
                else if (ID == 0 && bookCopy != null && member.Loans.Count() >= member.Membership.MaxLoans)
                {
                    yield return new ValidationResult("The member has already borrowed maximum number of books.", new string[] { "MemberID" });
                }

            }
        }
    }
}