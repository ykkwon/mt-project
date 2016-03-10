using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    public class FingerprintsController : Controller
    {
        readonly IFingerprintRepository _repository = new FingerprintRepository();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetFingerprintsByTitle(string inputTitle)
        {
            if (_repository.GetFingerprintsByTitle(inputTitle) != null)
            {
                return PartialView(_repository.GetFingerprintsByTitle(inputTitle));
            }
            else
            {
                return null;
            }

        }

        public ActionResult GetSingleFingerprintByHash(string inputHash)
        {
            return PartialView(_repository.GetSingleFingerprintByHash(inputHash));
        }
    }
}