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
    public class CalendarController : ApiController
    {
        // GET: api/Calendar


        public void Get()
        {

        }

        // GET: api/Calendar/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Calendar
        [Route("getTasksForCalendar")]
        [HttpPost]
        public IHttpActionResult Post([FromBody] string id)
        {
            DailyHelpMeDbContext db = new DailyHelpMeDbContext();

            List<DateTime> datess = db.RegisteredTo.Where(x => x.ID == id).Select(z => z.TaskInDates.TaskDate).ToList();
            List<DateAndTasks> list = new List<DateAndTasks>();

            db.InterestedInRegistered.Where(x => x.ID == id).Select(z => z.TaskInDates.TaskDate).ToList().ForEach(x =>
            {
                datess.Add(x);
            });

            datess = datess.Select(x => x).Distinct().ToList();

            foreach (var date in datess)
            {
                List<TaskAndUser> taskAndUserList = db.RegisteredTo.Where(x => x.ID == id && x.TaskInDates.TaskDate == date).Select(x => x.TaskInDates.Task).ToList()
                    .Select(x => new TaskAndUser
                    {
                        CityName = x.City.CityName,
                        TaskNumber = x.TaskNumber,
                        TaskName = x.TaskName,
                        TaskHour = x.TaskHour,
                        TaskDescription = x.TaskDescription,
                        MobilePhone = x.Request.Users.MobilePhone,
                        FirstName = x.Request.Users.FirstName,
                        SignStatus = "signed",
                        Photo = x.Request.Users.Photo,
                        Lat = x.Lat,
                        Lng = x.Lng,
                    }).ToList();

                List<Task> taskList = db.InterestedInRegistered.Where(x => x.ID == id && x.TaskInDates.TaskDate == date).Select(x => x.TaskInDates.Task).ToList();


                foreach(var x in taskList)
                {
                    taskAndUserList.Add(new TaskAndUser
                    {
                        CityName = x.City.CityName,
                        TaskNumber = x.TaskNumber,
                        TaskName = x.TaskName,
                        TaskHour = x.TaskHour,
                        TaskDescription = x.TaskDescription,
                        MobilePhone = x.Request.Users.MobilePhone,
                        FirstName = x.Request.Users.FirstName,
                        SignStatus = "wait",
                        Photo = x.Request.Users.Photo,
                        Lat = x.Lat,
                        Lng = x.Lng,

                    });
                };


                //db.InterestedInRegistered.Where(x => x.ID == id && x.TaskInDates.TaskDate == date).Select(x => x.TaskInDates.Task).ToList()
                //    .ForEach(x=> boo.Add(new TaskAndUser
                //    {
                //        CityName = x.City.CityName,
                //        TaskNumber = x.TaskNumber,
                //        TaskName = x.TaskName,
                //        TaskHour = x.TaskHour,
                //        TaskDescription = x.TaskDescription,
                //        MobilePhone = x.Request.Users.MobilePhone,
                //        FirstName = x.Request.Users.FirstName,
                //        SignStatus = "wait",
                //        Photo = x.Request.Users.Photo,
                //        Lat = x.Lat,
                //        Lng = x.Lng,

                //    }));

                list.Add(new DateAndTasks
                {
                    Date = date,
                    TaskList = taskAndUserList
                });
            };

            if (list.Count == 0)
            {
                return Ok("empty");
            }
            return Ok(list.OrderBy(x=> x.Date));
        }

        // PUT: api/Calendar/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Calendar/5
        public void Delete(int id)
        {
        }
    }
}
