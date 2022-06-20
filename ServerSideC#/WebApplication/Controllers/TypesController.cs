using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DailyHelpMe;

namespace WebApplication.Controllers
{
    public class TypesController : ApiController
    { 
        [HttpGet]
        public IHttpActionResult Get()
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                return Ok(db.VolunteerType.Select(v => v.VolunteerName).ToList());
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [Route("getTypes")]
        [HttpGet]
        public IHttpActionResult GetTypes()
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                return Ok(db.VolunteerType.Select(x => new
                {
                    x.VolunteerCode,
                    x.VolunteerName,
                }).ToList());
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

    }
}
