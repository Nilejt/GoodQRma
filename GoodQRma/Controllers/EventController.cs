﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GoodQRma.Models;
using Microsoft.AspNet.Identity;

namespace GoodQRma.Controllers
{
    public class EventController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Event
        public ActionResult Index()
        {
            return View(db.Events.ToList());
        }

        // GET: Event/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);

            if (@event == null)
            {
                return HttpNotFound();
            }
            string currentUserID = User.Identity.GetUserId();

            var currentUser = (from u in db.Users
                               where u.Id == currentUserID
                               select u).Include("Events").Single();
            /*
            if(currentUser.Events.Contains(@event))
            {
                ViewBag.Here = "Opt Out";
            }
            else
            {
                ViewBag.Here = "Volunteer!";
            }
            */

            ViewBag.Here = currentUser.Events.Contains(@event) ? "Opt Out" : "Volunteer!";

            return View(@event);
        }

        //POST: Event/Details
        [HttpPost]
        [ActionName("Details")]
        public ActionResult ChangeAttendance(int? id)
        {
            var idid = User.Identity.GetUserId();
            ApplicationUser user = db.Users.Single(u => u.Id == idid);
            Event @event = db.Events.Single(e => e.eventID == id);

            if(user.Events.Contains(@event))
            {
                @event.Users.Remove(user);
                user.Events.Remove(@event);
                ViewBag.Here = "Volunteer!";

                db.SaveChanges();
            }
            else
            {
                @event.Users.Add(user);
                user.Events.Add(@event);
                ViewBag.Here = "Leave";

                db.SaveChanges();
            }

            return View(@event);
        }

        // GET: Event/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Event/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "eventID,userID,image,name,description,eventType,eventDate,eventTime,numVolunteersNeeded,address1,city,state,zipCode,country,contact,phone,eventURL")] Event @event)
        {
            if (ModelState.IsValid)
            {
                db.Events.Add(@event);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(@event);
        }

        // GET: Event/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Event/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "eventID,userID,image,name,description,eventType,eventDate,eventTime,numVolunteersNeeded,address1,city,state,zipCode,country,contact,phone,eventURL")] Event @event)
        {
            if (ModelState.IsValid)
            {
                db.Entry(@event).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(@event);
        }

        // GET: Event/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Events.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Event @event = db.Events.Find(id);
            db.Events.Remove(@event);
            db.SaveChanges();
            return RedirectToAction("Index");
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
