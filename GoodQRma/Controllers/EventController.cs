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

     [Authorize]
    public class EventController : Controller
    {

       
         
        ApplicationDbContext db = new ApplicationDbContext(); 

          
        // GET: Event
          [AllowAnonymous]
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

            db.Events.Include(s => s.Files).SingleOrDefault(s => s.eventID == id);

            if (@event == null)
            {
                return HttpNotFound();
            }

            string currentUserID = User.Identity.GetUserId();
            var currentUser = db.Users.Single(u => u.Id == currentUserID);

            ViewBag.Here = currentUser.Events.Contains(@event) ? "Leave" : "Volunteer!";

            return View(@event);
        }

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
        [Authorize (Roles="Member")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Event/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "eventID,image,name,description,eventType,eventDate,eventTime,numVolunteersNeeded,address1,city,state,zipCode,country,contact,phone,eventURL")] Event @event, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {

                
                //Ties the logged in USER to the created EVENT
                @event.userID = User.Identity.GetUserId();


                if (upload != null && upload.ContentLength > 0)
                {
                    var avatar = new File
                    {
                        FileName = System.IO.Path.GetFileName(upload.FileName),
                        FileType = FileType.Avatar,
                        ContentType = upload.ContentType
                    };
                    using (var reader = new System.IO.BinaryReader(upload.InputStream))
                    {
                        avatar.Content = reader.ReadBytes(upload.ContentLength);
                    }
                    @event.Files = new List<File> { avatar };
                }

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


        //[Authorize(Roles = "Member")]
        public ActionResult EventPoster(int? id)
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

            
            return View("EventPoster", "_Layout2", @event);
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
