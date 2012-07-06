using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MiniMe.Models;

namespace MiniMe.Controllers
{ 
    public class LinkController : Controller
    {
        private LinkDBContext db = new LinkDBContext();

        //
        // GET: /Link/

        public ViewResult Index()
        {
            return View(db.Links.ToList());
        }

        //
        // GET: /Link/Details/5

        public ViewResult Details(Guid id)
        {
            Link link = db.Links.Find(id);
            return View(link);
        }

        //
        // GET: /Link/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Link/Create

        [HttpPost]
        public ActionResult Create(Link link)
        {
            if (ModelState.IsValid)
            {
                link.LinkID = Guid.NewGuid();
                db.Links.Add(link);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(link);
        }
        
        //
        // GET: /Link/Edit/5
 
        public ActionResult Edit(Guid id)
        {
            Link link = db.Links.Find(id);
            return View(link);
        }

        //
        // POST: /Link/Edit/5

        [HttpPost]
        public ActionResult Edit(Link link)
        {
            if (ModelState.IsValid)
            {
                db.Entry(link).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(link);
        }

        //
        // GET: /Link/Delete/5
 
        public ActionResult Delete(Guid id)
        {
            Link link = db.Links.Find(id);
            return View(link);
        }

        //
        // POST: /Link/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {            
            Link link = db.Links.Find(id);
            db.Links.Remove(link);
            db.SaveChanges();
            return RedirectToAction("Index");
        }



        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}