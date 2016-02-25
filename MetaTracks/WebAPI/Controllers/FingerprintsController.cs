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
        IFingerprintRepository repo = new FingerprintRepository();
        public string GetSingleFingerprint(string inputHash)
        { 
            return repo.GetFingerprintByHash(inputHash);
        }
    }

   
}