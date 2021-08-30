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
using LMS.ViewModels;

namespace LMS.Controllers
{
    public class BooksController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        
        public ActionResult Index(string searchString, bool? copyAvailable, int? sortOrder, int? page)
        {
			int pageSize = 10;
			int pageNumber = (page ?? 1);
            var books = db.Books.Include(b => b.Press).Include(b => b.Publisher).Include(b => b.Authors);

            ViewBag.SearchString = searchString;
            ViewBag.CopyAvailable = copyAvailable;
            ViewBag.SortOrderDesc = sortOrder == 1;

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();
                books = books.Where(b => b.Name.ToLower().Contains(searchString) || b.Publisher.Name.ToLower().Contains(searchString) || b.Authors.Any(author => author.FirstName.ToLower().Contains(searchString) || author.MiddleName.ToLower().Contains(searchString) || author.LastName.ToLower().Contains(searchString)));
            }

            if(copyAvailable == true)
            {
                books = books.Where(b => b.Copies.Any(c => c.Available));
            }

            if (!ViewBag.SortOrderDesc)
            {
                books = books.OrderBy(b => b.PublishedDate);
            }
            else
            {
                books = books.OrderByDescending(b => b.PublishedDate);
            }

            return View(books.ToPagedList(pageNumber, pageSize));
        }

        
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.PressID = new SelectList(db.Press, "ID", "Name");
            ViewBag.PublisherID = new SelectList(db.Publishers, "ID", "Name");

            PopulateAssignedAuthorData(null);
            PopulateAssignedCategoryData(null);
            return View();
        }

        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,ISBN,PublisherID,PressID,PublishedDate,Charge,PenaltyCharge")] Book book, string[] selectedAuthors, string[] selectedCategories)
        {
            if (ModelState.IsValid)
            {
                db.Books.Add(book);
                UpdateAssignedAuthorData(selectedAuthors, ref book);
                UpdateAssignedCategoryData(selectedCategories, ref book);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PressID = new SelectList(db.Press, "ID", "Name", book.PressID);
            ViewBag.PublisherID = new SelectList(db.Publishers, "ID", "Name", book.PublisherID);
            PopulateAssignedAuthorData(null);
            PopulateAssignedCategoryData(null);

            return View(book);
        }

        
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.PressID = new SelectList(db.Press, "ID", "Name", book.PressID);
            ViewBag.PublisherID = new SelectList(db.Publishers, "ID", "Name", book.PublisherID);
            PopulateAssignedAuthorData(book);
            PopulateAssignedCategoryData(book);
            return View(book);
        }

        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,ISBN,PublisherID,PressID,PublishedDate,Charge,PenaltyCharge")] Book book, string[] selectedAuthors, string[] selectedCategories)
        {
            if (db.Entry(book).State == EntityState.Detached)
            {
                db.Books.Attach(book);
            }

            db.Entry(book).Collection(b => b.Authors).Load();
            db.Entry(book).Collection(b => b.Categories).Load();

            if (ModelState.IsValid)
            {
                UpdateAssignedAuthorData(selectedAuthors, ref book);
                UpdateAssignedCategoryData(selectedCategories, ref book);
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PressID = new SelectList(db.Press, "ID", "Name", book.PressID);
            ViewBag.PublisherID = new SelectList(db.Publishers, "ID", "Name", book.PublisherID);
            PopulateAssignedAuthorData(book);
            PopulateAssignedCategoryData(book);
            return View(book);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private void PopulateAssignedAuthorData(Book book)
        {
            var allAuthors = db.Authors;
            var bookAuthors = new HashSet<int>();
            if (book != null)
            {
                bookAuthors = new HashSet<int>(book.Authors.Select(s => s.ID));
            }

            var viewModel = new List<AssignedAuthorData>();
            foreach (Author author in allAuthors)
            {
                viewModel.Add(new AssignedAuthorData
                {
                    AuthorID = author.ID,
                    Name = author.FirstName + " " + author.MiddleName + " " + author.LastName,
                    Assigned = bookAuthors.Contains(author.ID)
                });
            }

            ViewBag.Authors= viewModel;
        }

        private void UpdateAssignedAuthorData(string[] selectedAuthors, ref Book book)
        {
            if(book.Authors == null)
            {
                book.Authors = new List<Author>();
            }

            if (selectedAuthors == null)
            {
                book.Authors = new List<Author>();
                return;
            }

            var selectedAuthorsHS = new HashSet<string>(selectedAuthors);
            var bookAuthors = new HashSet<int>(book.Authors.Select(c => c.ID));
            foreach (var author in db.Authors)
            {
                if (selectedAuthorsHS.Contains(author.ID.ToString()))
                {
                    if (!bookAuthors.Contains(author.ID))
                    {
                        book.Authors.Add(author);
                    }
                }
                else
                {
                    if (bookAuthors.Contains(author.ID))
                    {
                        book.Authors.Remove(author);
                    }
                }
            }
        }

        private void PopulateAssignedCategoryData(Book book)
        {
            var allCategories = db.Categories;
            var bookCategories = new HashSet<int>();
            if (book != null)
            {
                bookCategories = new HashSet<int>(book.Categories.Select(s => s.ID));
            }

            var viewModel = new List<AssignedCategoryData>();
            foreach (Category category in allCategories)
            {
                viewModel.Add(new AssignedCategoryData
                {
                    CategoryID = category.ID,
                    Name = category.Name,
                    Assigned = bookCategories.Contains(category.ID)
                });
            }

            ViewBag.Categories = viewModel;
        }

        private void UpdateAssignedCategoryData(string[] selectedCategories, ref Book book)
        {
            if (book.Categories == null)
            {
                book.Categories = new List<Category>();
            }

            if (selectedCategories == null)
            {
                book.Categories = new List<Category>();
                return;
            }

            var selectedCategoriesHS = new HashSet<string>(selectedCategories);
            var bookCategories = new HashSet<int>(book.Categories.Select(c => c.ID));
            foreach (var category in db.Categories)
            {
                if (selectedCategoriesHS.Contains(category.ID.ToString()))
                {
                    if (!bookCategories.Contains(category.ID))
                    {
                        book.Categories.Add(category);
                    }
                }
                else
                {
                    if (bookCategories.Contains(category.ID))
                    {
                        book.Categories.Remove(category);
                    }
                }
            }
        }
    }
}