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
                return Ok(db.VolunteerType.Where(type => type.Aprroved == true).Select(v => v.VolunteerName).ToList());
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [Route("acceptType")]
        [HttpPost]
        public IHttpActionResult AcceptType([FromBody] string type)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                int typeCode =int.Parse(type);

                VolunteerType vol = db.VolunteerType.FirstOrDefault(x=> x.VolunteerCode == typeCode);
                if (vol is null)
                {
                    return Ok("NO");
                }
                if (vol.Aprroved == true)
                {
                    vol.Aprroved = false;
                }
                else {
                    vol.Aprroved = true;
                }

                string aprroved = (bool)vol.Aprroved ? "מאושר" : "לא מאושר";


                db.SaveChanges();
                return Ok(new string[3] { vol.VolunteerName, aprroved, vol.VolunteerCode.ToString() });
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [Route("addType")]
        [HttpPost]
        public IHttpActionResult AddType([FromBody] string type)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                db.VolunteerType.Add(
                    new VolunteerType
                    {
                        VolunteerName = type,
                        Aprroved = false
                    });
                db.SaveChanges();
                return Ok("OK");
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
