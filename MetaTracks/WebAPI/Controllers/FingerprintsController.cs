using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    public class FingerprintsController : Controller
    {
        IFingerprintRepository repository = new FingerprintRepository();

        [System.Web.Mvc.Authorize]
 
        public ActionResult GetSingleFingerprintByHash(string inputHash)
        { 
            return PartialView(repository.GetSingleFingerprintByHash(inputHash));
        }

        [System.Web.Mvc.Authorize]

        public ActionResult GetFingerprintsByTitle(string inputTitle)
        {
            return PartialView(repository.GetFingerprintsByTitle(inputTitle));
        }
    }

   
}