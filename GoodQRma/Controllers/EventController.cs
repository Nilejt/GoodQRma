using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GoodQRma.Models;
using Microsoft.AspNet.Identity;
using PagedList.Mvc;
using PagedList;
using System.Net.Mail;


namespace GoodQRma.Controllers
{

    [Authorize]
    public class EventController : Controller
    {

        ApplicationDbContext db = new ApplicationDbContext();


        // GET: Event
        [AllowAnonymous]

        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.currentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var events = from v in db.Events
                         select v;

            var events1 = from v in db.Events
                         select v;

            if (!string.IsNullOrEmpty(searchString))
            {
                events1 = events.Where(v => v.eventType.Contains(searchString));
                events = events.Where(v => v.name.Contains(searchString)).Union(events1);

            }

            switch (sortOrder)
            {
                case "name_desc":
                    events = events.OrderByDescending(v => v.eventType);
                    break;
                case "Date":
                    events = events.OrderBy(v => v.eventDate);
                    break;
                case "date_desc":
                    events = events.OrderByDescending(v => v.eventDate);
                    break;
                default:
                    events = events.OrderBy(v => v.eventType);
                    break;

            }

            int pageSize = 3;
            int pageNumber = (page ?? 1);


            return View(events.ToPagedList(pageNumber, pageSize));
        }

        // GET: Event/Details/5
        [AllowAnonymous]
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

            if (User.Identity.IsAuthenticated && !User.IsInRole("Admin"))
            {
                string currentUserID = User.Identity.GetUserId();
                var currentUser = db.Users.Single(u => u.Id == currentUserID);

                ViewBag.Here = currentUser.Events.Contains(@event) ? "Leave" : "Volunteer!";
            }
            else
                ViewBag.Here = "Volunteer!";

            return View(@event);
        }

        //POST Details
        [HttpPost]
        [ActionName("Details")]
        public ActionResult ChangeAttendance(int? id)
        {
            if (User.Identity.IsAuthenticated)
            {
                //Get current user's id
                var idid = User.Identity.GetUserId();

                //Find current event and current user
                ApplicationUser user = db.Users.Single(u => u.Id == idid);
                Event @event = db.Events.Single(e => e.eventID == id);

                //SMTP Email set up
                MailMessage msg = new MailMessage("goodqrma@gmail.com", @user.Email);
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("goodqrma@gmail.com", "4goodqrma");



                if (user.Events.Contains(@event))
                {
                    //Add user as attending event
                    @event.Users.Remove(user);
                    user.Events.Remove(@event);

                    //Change button to join event
                    ViewBag.Here = "Volunteer!";

                    //Save database changes
                    db.SaveChanges();

                    //Send confirmation email
                    msg.Subject = @event.name + " Confirmation";
                    msg.Body = "Hello, " + user.Name + "\n\nYou are no longer attending " + @event.name + " on " + @event.eventDate + " at " + @event.eventTime + "\n\nThank You, \nGoodQRma";
                    client.Send(msg);
                }
                else
                {
                    //Add user as attending event
                    @event.Users.Add(user);
                    user.Events.Add(@event);

                    //Change button to leave event
                    ViewBag.Here = "Opt Out";

                    //Save database changes
                    db.SaveChanges();

                    //Send confirmation email
                    msg.Subject = @event.name + " Confirmation";
                    msg.Body = "Hello, " + user.Name + "\n\nYou are confirmed to attend " + @event.name + " on " + @event.eventDate + " at " + @event.eventTime + "\n\nThank You, \nGoodQRma";
                    client.Send(msg);
                }

                return View(@event);
            }
            else
                return View("Login", "Account");

        }

        // GET: Event/Create
        [Authorize(Roles = "Member")]
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

            if (@event.userID == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                db.Events.Remove(@event);
                db.SaveChanges();
            }
            
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
