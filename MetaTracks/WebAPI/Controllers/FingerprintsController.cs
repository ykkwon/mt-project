using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
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

        public string GetSingleFingerprintByHash(string inputHash)
        {
            var returnedString = _repository.GetSingleFingerprintByHash(inputHash);
            if (returnedString != null)
            {
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                message.Content = new StringContent(returnedString, Encoding.UTF8);
                return message.Content.ReadAsStringAsync().Result;
            }
            return null;
        }

        public string GetAllTitlesSQL()
        {
            var returnedString = _repository.GetAllTitlesSQL();
            if (returnedString != null)
            {
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                message.Content = new StringContent(returnedString, Encoding.UTF8);
                return message.Content.ReadAsStringAsync().Result;
            }
            return null;
        }
    }
}