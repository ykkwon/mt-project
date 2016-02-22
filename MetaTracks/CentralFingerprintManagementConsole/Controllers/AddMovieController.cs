using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CentralFingerprintManagementConsole.Controllers
{
    public class AddMovieController : Controller
    {
        // GET: AddMovie
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}