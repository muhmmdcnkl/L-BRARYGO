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

namespace LMS.Controllers
{
    public class BookCopiesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        
        public ActionResult Index(string searchString, int? page)
        {
			int pageSize = 10;
			int pageNumber = (page ?? 1);

            var bookCopies = db.BookCopies.Include(b => b.Book);

            ViewBag.SearchString = searchString;

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();

                int number = 0;
                int.TryParse(searchString, out number);

                if (number > 0)
                {
                    bookCopies = bookCopies.Where(bc => bc.ID.ToString() == searchString);
                }
                else
                {
                    bookCopies = bookCopies.Where(bc => bc.Book.Name.ToLower().Contains(searchString) || bc.Book.Publisher.Name.ToLower().Contains(searchString) || bc.Book.Authors.Any(author => author.FirstName.ToLower().Contains(searchString) || author.MiddleName.ToLower().Contains(searchString) || author.LastName.ToLower().Contains(searchString)));
                }
            }

            return View(bookCopies.OrderByDescending(m => m.Book.Name).ToPagedList(pageNumber, pageSize));
        }

        // GET: BookCopies/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookCopy bookCopy = db.BookCopies.Find(id);
            if (bookCopy == null)
            {
                return HttpNotFound();
            }
            return View(bookCopy);
        }

        
        [Authorize]
        public ActionResult Create(int? SelectedBookID)
        {
            ViewBag.BookID = new SelectList(db.Books, "ID", "Name", SelectedBookID);
            return View();
        }

        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,BookID,Location,Available")] BookCopy bookCopy)
        {
            if (ModelState.IsValid)
            {
                bookCopy.AddedOn = DateTime.Now;
                db.BookCopies.Add(bookCopy);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BookID = new SelectList(db.Books, "ID", "Name", bookCopy.BookID);
            return View(bookCopy);
        }

        
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookCopy bookCopy = db.BookCopies.Find(id);
            if (bookCopy == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookID = new SelectList(db.Books, "ID", "Name", bookCopy.BookID);
            return View(bookCopy);
        }

        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,BookID,Location,Available")] BookCopy bookCopy)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bookCopy).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookID = new SelectList(db.Books, "ID", "Name", bookCopy.BookID);
            return View(bookCopy);
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
