using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CentralFingerprintManagementConsole.Controllers
{
    public class ActivityController : Controller
    {
        // GET: Activity
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}