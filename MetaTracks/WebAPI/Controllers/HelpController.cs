using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebAPI.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}