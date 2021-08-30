using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using PagedList;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LMS.Models;
using System.Data.Entity.Validation;
using System.Text;
using Microsoft.AspNet.Identity;

namespace LMS.Controllers
{
    [Authorize]
    public class LoansController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        
        public ActionResult Index(int? page, bool? showReturnedBooks)
        {
			int pageSize = 10;
			int pageNumber = (page ?? 1);
            ViewBag.ShowReturnedBooks = showReturnedBooks;

            var loans = db.Loans.Include(l => l.BookCopy)
                .Include(l => l.LoanType)
                .Include(l => l.Member);

            if (showReturnedBooks != true)
            {
                loans = loans.Where(b => b.ReturnedOn == null);
            }

            var loanslist = loans.OrderByDescending(l => l.IssuedOn.Year)
                .ThenByDescending(l => l.IssuedOn.Month)
                .ThenByDescending(l => l.IssuedOn.Day)
                .ThenBy(l => l.BookCopy.Book.Name)
                .ToPagedList(pageNumber, pageSize);

            return View(loanslist);
        }

        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Loan loan = db.Loans.Find(id);
            if (loan == null)
            {
                return HttpNotFound();
            }
            return View(loan);
        }

        
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookCopy copy = db.BookCopies.Where(c => c.Available && c.ID == id).FirstOrDefault();
            if (copy == null)
            {
                return HttpNotFound();
            }

            ViewBag.BookCopy = copy;

            ViewBag.BookCopyID = id;
            ViewBag.LoanTypeID = new SelectList(db.LoanTypes, "ID", "Name");

            var members = db.Members.Select(m => new {
                ID = m.ID,
                Name = m.ID + " - " +  m.FirstName + " " + m.MiddleName + " "+ m.LastName
            }).ToList();
            ViewBag.MemberID = new SelectList(members, "ID", "Name");

            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int? id, [Bind(Include = "MemberID,LoanTypeID,BookCopyID")] Loan loan)
        {
            if (id == null || id != loan.BookCopyID)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            BookCopy copy = db.BookCopies.Where(c => c.Available && c.ID == id).FirstOrDefault();
            if (copy == null)
            {
                return HttpNotFound();
            }

            ViewBag.BookCopy = copy;

            loan.BookCopy = copy;
            var loanType = db.LoanTypes.Find(loan.LoanTypeID);

            var dateIssued = DateTime.Now;
            var dueDate = dateIssued.AddDays(loanType.Duration);
            var dateReturned = DateTime.MinValue;
            loan.IssuedOn = dateIssued;
            loan.DueDate = dueDate;
            loan.LoanCharge = loanType.Duration * copy.Book.Charge;
            loan.PenaltyCharge = 0;
            loan.LoanedBy = this.db.Users.Find(User.Identity.GetUserId());

            if (ModelState.IsValid)
            {
                db.Loans.Add(loan);
                copy.Available = false;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = loan.ID });
            }

            ViewBag.BookCopyID = id;
            ViewBag.LoanTypeID = new SelectList(db.LoanTypes, "ID", "Name");

            var members = db.Members.Select(m => new {
                ID = m.ID,
                Name = m.ID + " - " + m.FirstName + " " + m.MiddleName + " " + m.LastName
            }).ToList();
            ViewBag.MemberID = new SelectList(members, "ID", "Name");
            return View(loan);
        }

        
        public ActionResult Return(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookCopy copy = db.BookCopies.Where(c => !c.Available && c.ID == id).FirstOrDefault();
            Loan loan = copy.Loans.LastOrDefault();
            if (copy == null || loan == null)
            {
                return HttpNotFound();
            }

            loan.ReturnedOn = DateTime.Now;
            TimeSpan difference = loan.ReturnedOn.GetValueOrDefault() - loan.DueDate.GetValueOrDefault();
            loan.PenaltyCharge = loan.ReturnedOn < loan.DueDate ? 0 : Math.Round(difference.TotalDays) * copy.Book.PenaltyCharge;
            copy.Available = true;
            db.SaveChanges();

            return RedirectToAction("Details", new { id = loan.ID });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
