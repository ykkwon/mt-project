using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    public class FingerprintsController : Controller
    {
        static readonly IFingerprintRepository repository = new FingerprintRepository();

        public IEnumerable<Fingerprint> GetAllFingerprints()
        {
            return repository.GetAll();
        }

        public string GetProduct(int id)
        {
            string item = repository.Get(id);
            if (item == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return item.ToString();
        }
    }

   
}