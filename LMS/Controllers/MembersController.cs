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
    [Authorize]
    public class MembersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        
        public ActionResult Index(string searchString, int? page)
        {
			int pageSize = 10;
			int pageNumber = (page ?? 1);
            var members = db.Members.Include(m => m.Membership);

            ViewBag.SearchString = searchString;

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim();

                int number = 0;
                int.TryParse(searchString, out number);

                if (number > 0)
                {
                    members = members.Where(m => m.ID.ToString() == searchString);
                }else
                {
                    members = members.Where(m => m.FirstName.ToLower().Contains(searchString) || m.MiddleName.ToLower().Contains(searchString) || m.LastName.ToLower().Contains(searchString));
                }
            }

            return View(members.OrderBy(m => m.FirstName).ToPagedList(pageNumber, pageSize));
        }

        
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            return View(member);
        }

        
        public ActionResult Create()
        {
            ViewBag.MembershipID = new SelectList(db.Memberships, "ID", "Name");
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,FirstName,MiddleName,LastName,DateOfBirth,MembershipID,Address,PhoneNumber")] Member member)
        {
            if (ModelState.IsValid)
            {
                db.Members.Add(member);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MembershipID = new SelectList(db.Memberships, "ID", "Name", member.MembershipID);
            return View(member);
        }

        
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Member member = db.Members.Find(id);
            if (member == null)
            {
                return HttpNotFound();
            }
            ViewBag.MembershipID = new SelectList(db.Memberships, "ID", "Name", member.MembershipID);
            return View(member);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,FirstName,MiddleName,LastName,DateOfBirth,MembershipID,Address,PhoneNumber")] Member member)
        {
            if (ModelState.IsValid)
            {
                db.Entry(member).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MembershipID = new SelectList(db.Memberships, "ID", "Name", member.MembershipID);
            return View(member);
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
