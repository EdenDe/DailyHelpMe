using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DailyHelpMe;
using WebApplication.Dto;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class AcceptVolController : ApiController
    {


        // POST: api/getVol
        [Route("getVol")]
        [HttpPost]
        public IHttpActionResult Post([FromBody] string id)
        {
            DailyHelpMeDbContext db = new DailyHelpMeDbContext();
            DateTime today = DateTime.Now;
            List<VolAndTask> list = new List<VolAndTask>();

            List<InterestedInRegistered> regiList = db.InterestedInRegistered.Where(x => x.TaskInDates.Task.Request.ID == id && x.TaskInDates.TaskDate > today).ToList();

            if (regiList.Count == 0)
            {
                return Ok(regiList);
            }

            regiList.Select(x => x.TaskInDates.Task).Where(x => x.Request.ID == id).Distinct().ToList().ForEach(x =>
            {
                VolAndTask yox = new VolAndTask
                {
                    TaskNumber = x.TaskNumber,
                    TaskName = x.TaskName,
                    TaskHour = x.TaskHour,
                    TaskDates = regiList.Where(y => y.TaskInDates.TaskNumber == x.TaskNumber).Select(z => z.TaskInDates.TaskDate).Distinct().ToList(),
                };

                yox.UsersRegistered = regiList.Where(z => z.TaskInDates.TaskNumber == x.TaskNumber)
                    .Select(z => new UsersPerDate
                    {
                        FirstName = z.Users.FirstName,
                        LastName = z.Users.LastName,
                        ID = z.Users.ID,
                        Photo = z.Users.Photo,
                        Date = z.TaskInDates.TaskDate,
                        TotalRate = z.Users.TotalRate,
                        Rank = z.Users.Rank,

                    }).ToList();

                list.Add(yox);
            });

            return Ok(list);
        }


        [Route("acceptVol")]
        [HttpPost]
        public IHttpActionResult AcceptVol([FromBody] TaskNumDate accept)
        {
            DailyHelpMeDbContext db = new DailyHelpMeDbContext();

            //int? num = db.TaskInDates.FirstOrDefault(x => x.TaskNumber == accept.TaskNumber && x.TaskDate == accept.TaskDate).TaskDateNum;

            List<InterestedInRegistered> regi = db.InterestedInRegistered.Where(x => x.TaskInDates.TaskNumber == accept.TaskNumber && x.TaskInDates.TaskDate == accept.TaskDate).ToList();

            if (regi == null)
            {
                return Ok("NO");
            }

            db.RegisteredTo.Add(new RegisteredTo
            {
                TaskDateNum = regi.First(x => x.ID == accept.ID).TaskDateNum,
                ID = accept.ID,
                RegisterStatus = "טרם בוצע",
            });

            regi.ForEach(x =>
            {
                db.InterestedInRegistered.Remove(x);
            });

            db.SaveChanges();
            return Ok("OK");
        }


        [Route("denyVol")]
        [HttpDelete]
        public IHttpActionResult DenyVol([FromBody] TaskNumDate accept)
        {
            DailyHelpMeDbContext db = new DailyHelpMeDbContext();

            InterestedInRegistered regi = db.InterestedInRegistered.FirstOrDefault(x => x.TaskInDates.TaskNumber == accept.TaskNumber && x.TaskInDates.TaskDate == accept.TaskDate);

            if (regi == null)
            {
                return Ok("NO");
            }

            db.InterestedInRegistered.Remove(regi);
            db.SaveChanges();

            return Ok("OK");
        }

    }
}
