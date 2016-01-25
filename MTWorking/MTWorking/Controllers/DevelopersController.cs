using MTWorking.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MTWorking.Controllers
{
    public class DevelopersController : ApiController
    {
        public IHttpActionResult Get()
        {
            IList<Developer> projectDevelopers = new List<Developer>();
            projectDevelopers.Add(new Developer() { Name = "Kristoffer", Role = "Cat lover", About = "Do you want a taco?" });
            projectDevelopers.Add(new Developer() { Name = "Glenn", Role = "Anarchist", About = "For the Watch." });
            projectDevelopers.Add(new Developer() { Name = "Kristian", Role = "Minister of Finance", About = "So yeah.. We're using Java, right?" });
            return Ok<IList<Developer>>(projectDevelopers);
        }
    }
}
