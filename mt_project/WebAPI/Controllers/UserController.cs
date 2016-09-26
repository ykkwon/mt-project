using System.Web.Mvc;
using System.Web.Security;

namespace WebAPI.Controllers
{
        public class UserController : Controller
        {
        //
        // GET: /User/
        [Authorize]
        public ActionResult Index()
            {
                return View();
            }

            [HttpGet]
            public ActionResult Login()
            {
                return View();
            }

            [HttpPost]
            public ActionResult Login(WebAPI.Models.User user)
            {
                if (ModelState.IsValid)
                {
                    if (user.IsValid(user.UserName, user.Password))
                    {
                        FormsAuthentication.SetAuthCookie(user.UserName, user.RememberMe);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Some required information is missing or incomplete. Please correct your entries and try again.");
                    }
                }
                return View(user);
            }
            public ActionResult Logout()
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("Index", "Home");
            }
        }
    }
