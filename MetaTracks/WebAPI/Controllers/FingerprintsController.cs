using System.Web.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    public class FingerprintsController : Controller
    {
        readonly IFingerprintRepository _repository = new FingerprintRepository();

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult GetFingerprintsByTitle(string inputTitle)
        {
            return PartialView(_repository.GetFingerprintsByTitle(inputTitle));
        }

        [Authorize]
        public ActionResult GetSingleFingerprintByHash(string inputHash)
        {
            return PartialView(_repository.GetSingleFingerprintByHash(inputHash));
        }
    }
}