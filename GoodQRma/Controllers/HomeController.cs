using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using GoodQRma.Models;

namespace GoodQRma.Controllers
{
    public class HomeController : Controller
    {
        ApplicationDbContext db = new ApplicationDbContext();


        public ActionResult Index()
        {

            var recentEvents = db.Events.OrderByDescending(e => e.eventDate).Select(e => e).Take(2);
            ViewData["event1"] = recentEvents.Take(1).Single();
            ViewData["event2"] = recentEvents.Skip(1).Single();

            return View("Index", "_LayoutHOME");
        }
       

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact us.";
            return View(new HelpForm());
        }
         
       
         [HttpPost] 
        public ActionResult Contact(GoodQRma.Models.HelpForm hform)
        {
           
            if (hform.Body != null && hform.From != null)
            {
                MailMessage mail = new MailMessage();
                mail.To.Add("goodqrma@gmail.com");
                mail.From = new MailAddress(hform.From);
                mail.Subject = hform.Subject;
                string Body = hform.Body;
                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential
                ("goodqrma@gmail.com", "4goodqrma");// Enter seders User name and password  
                smtp.EnableSsl = true;
                smtp.Send(mail);
                return RedirectToAction("Index");
            }

            else
            {
                return RedirectToAction("Index");
            }
        }
    }
}
 
       
       