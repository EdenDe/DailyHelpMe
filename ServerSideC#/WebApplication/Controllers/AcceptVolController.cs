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
            PushNotificationsController push = new PushNotificationsController();
            InterestedInRegistered taskDate = db.InterestedInRegistered.FirstOrDefault(x => x.TaskInDates.TaskNumber == accept.TaskNumber && x.TaskInDates.TaskDate == accept.TaskDate && x.ID == accept.ID);

            if (taskDate is null)
            {
                return Ok("NO");
            }
         
            RegisteredTo newRegi = new RegisteredTo
            {
                TaskDateNum = taskDate.TaskDateNum,
                ID = accept.ID,
                RegisterStatus = "טרם בוצע",
            };

            db.RegisteredTo.Add(newRegi);
            List<RegisteredTo> regiDBList = db.RegisteredTo.ToList();
            regiDBList.Add(newRegi);

            if (taskDate.Users.TokenID != null) {
                push.PushNoti(new PushNoteData
                {
                    to = taskDate.Users.TokenID,
                    title = "התנדבות אושרה",
                    body = $"התנדבותך למשימה {taskDate.TaskInDates.Task.TaskName} בתאריך {taskDate.TaskInDates.TaskDate.ToShortDateString()} אושרה",
                });
            }

            int numOfCurrentVol = regiDBList.Where(x => x.TaskDateNum == taskDate.TaskDateNum).Count();
            int numOfVolWanted = taskDate.TaskInDates.Task.NumOfVulRequired;

            List<InterestedInRegistered> regi = db.InterestedInRegistered.Where(x => x.TaskDateNum == taskDate.TaskDateNum && x.ID != accept.ID).ToList();

            db.InterestedInRegistered.Remove(taskDate);

            if (numOfCurrentVol == numOfVolWanted)
            {
                db.InterestedInRegistered.RemoveRange(regi);
            }

            db.SaveChanges();
            return Ok("OK");
        }


        [Route("denyVol")]
        [HttpDelete]
        public IHttpActionResult DenyVol([FromBody] TaskNumDate accept)
        {
            DailyHelpMeDbContext db = new DailyHelpMeDbContext();
            PushNotificationsController push = new PushNotificationsController();

            InterestedInRegistered regi = db.InterestedInRegistered.FirstOrDefault(x => x.TaskInDates.TaskNumber == accept.TaskNumber && x.TaskInDates.TaskDate == accept.TaskDate);

            if (regi == null)
            {
                RegisteredTo regi2 = db.RegisteredTo.FirstOrDefault(x => x.TaskInDates.TaskNumber == accept.TaskNumber && x.TaskInDates.TaskDate == accept.TaskDate);

                if (regi2 == null)
                {
                    return Ok("NO");
                }

                if (regi2.Users.TokenID != null) {
                    push.PushNoti(new PushNoteData
                    {
                        to = regi2.Users.TokenID,
                        title = "התנדבות נדחתה",
                        body = $"התנדבותך למשימה {regi2.TaskInDates.Task.TaskName} בתאריך {regi2.TaskInDates.TaskDate.ToShortDateString()} בוטלה",
                    });
                }        
                db.RegisteredTo.Remove(regi2);
            }
            else
            {
                push.PushNoti(new PushNoteData
                {
                    to = regi.Users.TokenID,
                    title = "בקשת התנדבות נדחתה",
                    body = $"התנדבותך למשימה {regi.TaskInDates.Task.TaskName} בתאריך {regi.TaskInDates.TaskDate.ToShortDateString()} נדחתה",
                });

                db.InterestedInRegistered.Remove(regi);
            }

            db.SaveChanges();

            return Ok("OK");
        }

    }
}
