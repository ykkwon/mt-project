using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
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

        [System.Web.Mvc.HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {

            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(HostingEnvironment.MapPath("~/App_Data/uploads"), fileName);
                _repository.WriteToMySql(path);
            }

            return RedirectToAction("Index");
        }

        public ActionResult GetFingerprintsByTitle(string inputTitle)
        {
            if (_repository.GetFingerprintsByTitle(inputTitle) != null)
            {
                return PartialView(_repository.GetFingerprintsByTitle(inputTitle));
            }
            return null;
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

        public string GetAllMediaTypesSQL()
        {
            var returnedString = _repository.GetAllMediaTypesSQL();
            if (returnedString != null)
            {
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                message.Content = new StringContent(returnedString, Encoding.UTF8);
                return message.Content.ReadAsStringAsync().Result;
            }
            return null;
        }

        public string GetAllFingerprintsSQL(string inputTitle)
        {
            var returnedString = _repository.GetAllFingerprintsSQL(inputTitle);
            if (returnedString != null)
            {
                HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
                message.Content = new StringContent(returnedString, Encoding.UTF8);
                return message.Content.ReadAsStringAsync().Result;
            }
            return null;
        }

        public string GetAllTimestampsSQL(string inputTitle)
        {
            var returnedString = _repository.GetAllTimestampsSQL(inputTitle);
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