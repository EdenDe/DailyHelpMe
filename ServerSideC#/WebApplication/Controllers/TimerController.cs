using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DailyHelpMe;

namespace WebApplication.Controllers
{
    public class TimerController : ApiController
    {
        //code for timer
        [HttpGet]
        [Route("api/start")]
        public void StartTimer()
        {
            WebApiApplication.StartTimer1();
        }

        //code for timer
        [HttpGet]
        [Route("api/stop")]
        public void StopTimer()
        {
            WebApiApplication.EndTimer1();
        }


        [HttpGet]
        [Route("api/yoyo")]
        public void CustomerLearner()
        {
            try
            {
         
                int thisMonth = DateTime.Now.Month;
                int amountVolNow = 0;
                double avgAmountOfVol = 0;
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                int howManyMonth = 2;
                List<Users> userList = db.Users.ToList();

                foreach (var user in userList)
                {
                    for (int i = 1; i <= howManyMonth; i++)
                    {
                        avgAmountOfVol += user.RegisteredTo.Where(task => task.TaskInDates.TaskDate.Month == DateTime.Now.Month - i).Count();
                    }

                    avgAmountOfVol /= 4;

                    amountVolNow = user.RegisteredTo.Where(task => task.TaskInDates.TaskDate.Month == thisMonth).Count();
                    amountVolNow += user.InterestedInRegistered.Where(task => task.TaskInDates.TaskDate.Month == thisMonth).Count();

                    if (avgAmountOfVol > amountVolNow)
                    {
              
                        List<string> TypeName = new List<string>();
                        List<string> topTypeName = new List<string>();

                        foreach (var task in user.RegisteredTo)
                        {
                            TypeName.Add(task.TaskInDates.Task.TaskTypes.GroupBy(type => type.VolunteerType.VolunteerName).Select(x =>
                                new { Type = x.Key, typesOfType = x.Count() }).OrderByDescending(y => y.typesOfType).First().Type);
                        }

                        topTypeName = TypeName.GroupBy(type => type).Select(x =>
                                 new { Type = x.Key, typesOfType = x.Count() }).OrderByDescending(y => y.typesOfType).Take(2).Select(x=> x.Type).ToList();

                        List<TaskInDates> taskDateList = db.TaskInDates.Where(task => task.TaskDate > DateTime.Today && task.Task.Request.ID != user.ID).ToList();

                        List<Task> taskList= taskDateList.Where(x => !x.RegisteredTo.Any()).Select(x=> x.Task).ToList();

                       //List<Task> taskList= db.Task.Where(task => task.Request.ID != user.ID && db.RegisteredTo.FirstOrDefault(x => x.TaskInDates.TaskNumber == task.TaskNumber) == null).ToList();

                        foreach (var typeName in topTypeName)
                        {
                            foreach (var task in taskList)
                            {

                                Task t = task.TaskTypes.Where(type => type.VolunteerType.VolunteerName == typeName).First().Task;
                                if (t != null)
                                { 
                                
                                }
                                // VolunteerType type = db.VolunteerType.First(x => x.VolunteerName == typeName);

                            }
                           
                        }

                       // Task taskChosen =  db.Task.Where(task1 => !db.RegisteredTo.Where(task2 => task2.TaskInDates.TaskNumber == task1.TaskNumber).Any()).Where(x=> x.TaskTypes.(topTypeName)) .First();

                    }
                }


            }
            catch (Exception)
            {
                return;
            }

        }
    }
}
