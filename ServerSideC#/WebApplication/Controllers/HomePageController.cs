using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApplication.Dto;
using DailyHelpMe;
using System.Collections;
using System.Web.Http.Cors;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class HomePageController : ApiController
    {

        Dictionary<string, Func<FilterRequestBy, List<Task>, List<Task>>> filterOptions = new Dictionary<string, Func<FilterRequestBy, List<Task>, List<Task>>>()
        {
            { "Volunteer", (filterBy, taskList) => {
                taskList = taskList.Where(task => task.TaskTypes.Any(type => type.VolunteerType.VolunteerName == filterBy.VolunteerName)).ToList();
                return taskList;
                }
            },
            { "City", (filterBy, taskList) => {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                taskList = taskList.Where(t => t.CityCode == db.City.First(x => x.CityName == filterBy.CityName).CityCode).ToList();
                return taskList;
                }
            },
            { "Coords", (filterBy, taskList) => {

                double maxLng, minLng, maxLat, minLat, km = 10;
                maxLat = double.Parse(filterBy.Coords.Lat) + (km / 110.574);
                maxLng = double.Parse(filterBy.Coords.Lng) + (km / 111.320 * Math.Cos(double.Parse(filterBy.Coords.Lat)));

                minLat = double.Parse(filterBy.Coords.Lat) - (km / 110.574);
                minLng = double.Parse(filterBy.Coords.Lng) - (km / 111.320 * Math.Cos(double.Parse(filterBy.Coords.Lat)));

               taskList = taskList.Where(x => (double.Parse(x.Lat) > minLat) && (double.Parse(x.Lat) < maxLat) && (double.Parse(x.Lng) > minLng) && (double.Parse(x.Lat) < maxLng)).ToList();

                return taskList;
                }
            },
            { "Link", (filterBy, taskList) => {

                return taskList;
             }
            },
        };

        public List<Requests> GetRequests(UserAndFilter filter)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                List<Requests> requestList = new List<Requests>();
                List<Request> reqList = new List<Request>();
                List<Tasks> tasksList = new List<Tasks>();
                List<Task> TaskDbList = new List<Task>();

                if (filter.FilterBy.Exists(x => x.Type == "Link"))
                {
                    string link = filter.FilterBy[0].Link;
                    Request req = db.Request.FirstOrDefault(request => request.Link == link);
                    if (req != null)
                    {
                        TaskDbList = req.Task.ToList();
                    }
                }
                else
                {
                    TaskDbList = db.Task.Where(x => x.Request.RequestStatus == "פעיל" && x.Request.ID != filter.ID && x.Request.PrivateRequest == false).ToList();
                }

                foreach (var filterBy in filter.FilterBy)
                {
                    TaskDbList = filterOptions[filterBy.Type](filterBy, TaskDbList);
                }

                TaskDbList.ForEach(task =>
                {
                    Tasks newTasks = new Tasks
                    {
                        TaskNumber = task.TaskNumber,
                        TaskName = task.TaskName,
                        TaskHour = task.TaskHour,
                        DatesForTask = task.TaskInDates.Where(x => x.TaskDate > DateTime.Today).Select(y => y.TaskDate).OrderBy(date => date).ToList(),
                        TaskDescription = task.TaskDescription,
                        Confirmation = task.Confirmation,
                        RequestCode = task.RequestCode,
                        Lat = task.Lat,
                        Lng = task.Lng,
                        CityName = db.City.FirstOrDefault(y => y.CityCode == task.CityCode).CityName,
                        TypesList = task.TaskTypes.Where(type=> type.VolunteerType.Aprroved==true).Select(type => type.VolunteerType.VolunteerName).ToList(),
                        TaskDateStatus = task.TaskInDates.Where(x => x.TaskDate > DateTime.Today).Select(taskDate => new TaskStatus
                        {
                            TaskDateNum = taskDate.TaskDateNum,
                            TaskDate = taskDate.TaskDate,
                            Status = StatusSet(task.TaskNumber, taskDate.TaskDate, filter.ID)
                        }).ToList()

                    };

                    List<DateTime> toRemove = task.TaskInDates.Where(taskDate => task.NumOfVulRequired == db.RegisteredTo.Where(regi => regi.TaskDateNum == taskDate.TaskDateNum).Count())
                                                              .Select(x => x.TaskDate).ToList();

                    toRemove.ForEach(date =>
                    {
                        if (db.RegisteredTo.FirstOrDefault(x => x.TaskInDates.TaskNumber == task.TaskNumber && x.TaskInDates.TaskDate == date).ID != filter.ID)
                        {
                            newTasks.DatesForTask.Remove(date);
                            newTasks.TaskDateStatus.Remove(newTasks.TaskDateStatus.FirstOrDefault(taskDate => taskDate.TaskDate == date));
                        }
                    });

                    if (newTasks.DatesForTask.Any())
                        tasksList.Add(newTasks);

                });

                tasksList.ForEach(task =>
                {
                    reqList.Add(TaskDbList.FirstOrDefault(x => x.RequestCode == task.RequestCode).Request);
                });

                reqList = reqList.Distinct().ToList();

                reqList.ForEach(request =>
                {
                    List<string> types = new List<string>();
                    tasksList.Where(task => task.RequestCode == request.RequestCode).ToList().ForEach(task =>
                    {
                        task.TypesList.ForEach(type =>
                        {
                            types.Add(type);
                        });
                    });

                    requestList.Add(new Requests
                    {
                        UserUpload = request.Users.FirstName,   
                        LastName = request.Users.LastName,
                        RequestCode = request.RequestCode,
                        RequestName = request.RequestName,
                        ID = request.ID,
                        Rank = request.Users.Rank,
                        RequestStatus = request.RequestStatus,
                        Image = request.Users.Photo,
                        TypesList = types.Distinct().ToList(),
                        EndDate = tasksList.Where(task => task.RequestCode == request.RequestCode).Max(x => x.DatesForTask.Max(y => y.Date)),
                        StartDate = tasksList.Where(task => task.RequestCode == request.RequestCode).Min(x => x.DatesForTask.Min(y => y.Date)),
                        Task = tasksList.Where(task => task.RequestCode == request.RequestCode).ToList()
                    });
                });

                return requestList.OrderBy(x => x.StartDate).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        string StatusSet(int TaskNumber, DateTime date, string VolunteerID)
        {
            DailyHelpMeDbContext db = new DailyHelpMeDbContext();

            int taskDateNum = db.TaskInDates.FirstOrDefault(y => y.TaskNumber == TaskNumber && y.TaskDate == date).TaskDateNum;

            if (db.InterestedInRegistered.FirstOrDefault(x => taskDateNum == x.TaskDateNum && x.ID == VolunteerID) != null)
            {
                return "wait";
            }
            else if (db.RegisteredTo.FirstOrDefault(x => taskDateNum == x.TaskDateNum && x.ID == VolunteerID) != null)
            {
                return "cancel";
            }
            return "sign";
        }


        [Route("getRequests")]
        [HttpPost]
        public IHttpActionResult Post([FromBody] UserAndFilter sortInformation)
        {
            try
            {
                if (sortInformation.FilterBy is null)
                    sortInformation.FilterBy = new List<FilterRequestBy>();


                List<Requests> res = GetRequests(sortInformation);
                if (res is null)
                {
                    return Ok("Empty");
                }
                return Ok(res);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }


        [Route("signToTask")]
        [HttpPost]
        public IHttpActionResult Post([FromBody] TaskNumDate regi)
        {
            try
            {
                PushNotificationsController push = new PushNotificationsController();
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                TaskInDates taskChosen = db.TaskInDates.FirstOrDefault(task => task.TaskDate == regi.TaskDate && task.TaskNumber == regi.TaskNumber);
                string res = "NO";

                if (taskChosen == null)
                {
                    return Ok(res);
                }

                if (taskChosen.Task.Confirmation == true)
                {
                    db.InterestedInRegistered.Add(new InterestedInRegistered
                    {
                        ID = regi.ID,
                        TaskDateNum = taskChosen.TaskDateNum,
                        SignToTaskTime = DateTime.Now
                    });

                    if (taskChosen.Task.Request.Users.TokenID != null) {
                        push.PushNoti(new PushNoteData
                        {
                            to = taskChosen.Task.Request.Users.TokenID,
                            title = "שיבוץ חדש למשימה",
                            body = $" ישנו ממתין לאישור שיבוץ עבור המשימה {taskChosen.Task.TaskName} בתאריך {taskChosen.TaskDate.ToShortDateString()}",
                        });
                    }
                
                    res = "wait";
                }
                else
                {
                    db.RegisteredTo.Add(new RegisteredTo
                    {
                        RegisterStatus = "טרם בוצע",
                        ID = regi.ID,
                        TaskDateNum = taskChosen.TaskDateNum,
                    });

                    push.PushNoti(new PushNoteData
                    {
                        to = taskChosen.Task.Request.Users.TokenID,
                        title = "שיבוץ חדש למשימה",
                        body = $" ישנו שיבוץ חדש עבור המשימה {taskChosen.Task.TaskName} בתאריך {taskChosen.TaskDate.ToShortDateString()}",
                    });


                    res = "signed";
                }

                db.SaveChanges();

                return Ok(res);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, e.GetType());
            }
        }


        [Route("cancelTask")]
        [HttpDelete]
        public IHttpActionResult Delete([FromBody] TaskNumDate taskCancel)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                TaskInDates taskChosen = db.TaskInDates.FirstOrDefault(task => task.TaskDate == taskCancel.TaskDate && task.TaskNumber == taskCancel.TaskNumber);
                if (taskChosen == null)
                {
                    return Ok("NO");
                }

                InterestedInRegistered taskToCancel = db.InterestedInRegistered.FirstOrDefault(task => task.TaskDateNum == taskChosen.TaskDateNum);

                if (taskToCancel != null)
                {
                    db.InterestedInRegistered.Remove(taskToCancel);
                    db.SaveChanges();
                    return Ok("OK");
                }

                RegisteredTo taskToCancell = db.RegisteredTo.FirstOrDefault(task => task.TaskDateNum == taskChosen.TaskDateNum);

                if (taskToCancell != null)
                {
                    db.RegisteredTo.Remove(taskToCancell);
                    db.SaveChanges();
                    return Ok("OK");
                }

                return Ok("NO");
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, e.GetType());
            }
        }
    }
}
