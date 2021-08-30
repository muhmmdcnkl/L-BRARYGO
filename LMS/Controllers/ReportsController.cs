using LMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LMS.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        
        public ActionResult OldBooks()
        {
            var oneYearAgo = DateTime.Now.AddYears(-1);
            var copies = db.BookCopies
                .Where(bc => bc.AddedOn < oneYearAgo && bc.Available)
                .OrderBy(b => b.AddedOn)
                .ToList();

            return View(copies);
        }

        
        public ActionResult ClearOldBooks()
        {
            var oneYearAgo = DateTime.Now.AddYears(-1);
            var copies = db.BookCopies
                .Where(bc => bc.AddedOn < oneYearAgo && bc.Available)
                .ToList();

            db.BookCopies.RemoveRange(copies);
            
            db.SaveChanges();

            return RedirectToAction("OldBooks");
        }

        

        public ActionResult InactiveBooks()
        {
            var oneMonthAgo = DateTime.Now.AddDays(-31);

            var books = db.Books
                .Where(b => b.Copies.Any(c => !c.Loans.Any(l => l.IssuedOn > oneMonthAgo)))
                .ToList();

            return View(books);
        }

        
        public ActionResult InactiveMembers()
        {
            var oneMonthAgo = DateTime.Now.AddDays(-31);

            var members = db.Members.ToList();
            var inactiveMembers = new List<Member>();

            foreach(var member in members)
            {
                if(member.Loans.Count() == 0)
                {
                    inactiveMembers.Add(member);
                    continue;
                }

                var lastLoan = member.Loans.OrderByDescending(l => l.IssuedOn).First();
                if(lastLoan.IssuedOn < oneMonthAgo)
                {
                    inactiveMembers.Add(member);
                }
            }

            return View(inactiveMembers);
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