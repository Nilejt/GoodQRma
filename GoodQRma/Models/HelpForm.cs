﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoodQRma.Models
{
    public class HelpForm
    {
            public string To{ get; set; }
            public string From{ get; set; }
            //public string To { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
        

        //[Required, Display(Name = "Your name")]
        //public string FromName { get; set; }
        //[Required, Display(Name = "Your email"), EmailAddress]
        //public string FromEmail { get; set; }
        //[Required]
        //public string Message { get; set; }
    }
}
    