using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DailyHelpMe;
using WebApplication.Models;
using WebApplication.Controllers;
using WebApplication.Dto;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;

namespace WebApplication.Controllers
{
    public class AddRequestController : ApiController
    {

        [Route("addCity")]
        [HttpPost]
        public IHttpActionResult Post([FromBody] string cityName)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                if (db.City.FirstOrDefault(x => x.CityName == cityName) == null)
                {
                    db.City.Add(new City { CityName = cityName });
                    db.SaveChanges();
                }

                return Ok(db.City.FirstOrDefault(x => x.CityName == cityName).CityCode);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }


        [Route("addRequest")]
        [HttpPost]
        public IHttpActionResult Post([FromBody] EditRequest requestEdit)
        {
            try
            {
                Requests request = requestEdit.Request;
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                string link = "";
                Request req = new Request();

                List<Request> requestList = db.Request.ToList();

                if (request.RequestNew)
                {
                    while (true)
                    {
                        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                        link = new string(Enumerable.Repeat(chars, 20)
                           .Select(s => s[Random.Next(s.Length)]).ToArray());

                        if (!requestList.Any(r => r.Link == link))
                        {
                            break;
                        }
                    }
                }

                if (!request.RequestNew)
                {
                    req = requestList.FirstOrDefault(requ => requ.RequestCode == request.RequestCode);
                }

                req.RequestName = request.RequestNew ? request.RequestName : req.RequestName;
                req.ID = request.ID;
                req.RequestStatus = "פעיל";
                req.PrivateRequest = request.RequestNew ? request.PrivateRequest : req.PrivateRequest;
                req.Link = request.RequestNew ? link : req.RequestName;

                if (request.RequestNew)
                {
                    requestList.Add(req);
                }

                db.SaveChanges();

                foreach (var tasks in request.Task)
                {


                    //request.Task.ForEach(task =>
                    //{
                    Task task = new Task();
                    if (!tasks.New)
                    {
                        task = req.Task.FirstOrDefault(y => y.TaskNumber == tasks.TaskNumber);
                    }

                    task.TaskName = tasks.TaskName;
                    task.TaskDescription = tasks.TaskDescription;
                    task.NumOfVulRequired = tasks.NumOfVulRequired;
                    task.Confirmation = tasks.Confirmation;
                    task.CityCode = tasks.CityCode;
                    task.Lat = tasks.Lat;
                    task.Lng = tasks.Lng;
                    task.TaskHour = tasks.TaskHour;
                    task.RequestCode = req.RequestCode;

                    if (tasks.New)
                    {
                        db.Task.Add(task);
                    }

                    db.SaveChanges();

                    List<TaskTypes> typesToRemoveTask = new List<TaskTypes>();
                    List<string> typesToAdd = new List<string>();
                    List<DateTime> datesToAdd = new List<DateTime>();
                    List<DateTime> datesToRemove = new List<DateTime>();


                    tasks.TypesList.ForEach(type =>
                    {
                        if (!task.TaskTypes.Any(x => x.VolunteerType.VolunteerName != type) || tasks.New)
                        {
                            db.TaskTypes.Add(new TaskTypes
                            {
                                TaskNumber = task.TaskNumber,
                                VolunteerCode = db.VolunteerType.FirstOrDefault(y => y.VolunteerName == type).VolunteerCode
                            });
                        }

                    });

                    //task.TaskTypes.ToList().ForEach(type => {
                    ////    if (!tasks.TypesList.Contains(type.VolunteerType.VolunteerName)) {
                    ////        typesToAdd.Add(type.VolunteerType.VolunteerName);
                    ////    }                   
                    //});

                    //tasks.TypesList.ForEach(type => {
                    //    if (task.TaskTypes.Any(x => x.VolunteerType.VolunteerName != type)) {

                    //        typesToRemoveTask.Add(type);
                    //    }

                    //});

                    foreach (var date in tasks.DatesForTask)
                    {

                        if (db.Dates.SingleOrDefault(z => z.Date == date) == null)
                        {
                            db.Dates.Add(new Dates
                            {
                                Date = date
                            });
                            db.SaveChanges();
                        }

                        if (!task.TaskInDates.Any(taskDate => taskDate.TaskDate == date) || tasks.New)
                        {

                            db.TaskInDates.Add(new TaskInDates
                            {
                                TaskNumber = task.TaskNumber,
                                TaskDate = date
                            });
                        }
                    }

                    List<TaskInDates> taskDatesToRemove = new List<TaskInDates>();

                    if (!tasks.New)
                    {
                        task.TaskInDates.Where(x => !tasks.DatesForTask.Contains(x.TaskDate)).ToList().ForEach(x =>
                        {
                           // taskDatesToRemove.Add(x);
                            task.TaskInDates.Remove(x);
                        });

                        task.TaskTypes.Where(type => !tasks.TypesList.Contains(type.VolunteerType.VolunteerName)).ToList().ForEach(x => {

                            //typesToRemoveTask.Add(x);
                            task.TaskTypes.Remove(x);

                        });
                                               
                    }
                }

                db.SaveChanges();

                // return Ok(new { Status = "OK", Link = link });
                return Ok(req);


            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, e.GetType());
            }
        }

        private static Random Random = new Random();


        //[Route("addRequest")]
        //[HttpPost]
        //public IHttpActionResult Post([FromBody] Requests request)
        //{
        //    try
        //    {
        //        DailyHelpMeDbContext db = new DailyHelpMeDbContext();
        //        string link = "";

        //        while (true)
        //        {
        //        
        //            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        //            link = new string(Enumerable.Repeat(chars, 20)
        //               .Select(s => s[Random.Next(s.Length)]).ToArray());

        //            if (!db.Request.Any(r => r.Link == link))
        //            {
        //                break;
        //            }
        //        }

        //        Request req = new Request()
        //        {
        //            RequestName = request.RequestName,
        //            ID = request.ID,
        //            RequestStatus = request.RequestStatus,
        //            PrivateRequest = request.PrivateRequest,
        //            Link = link
        //        };

        //        db.Request.Add(req);
        //        db.SaveChanges();

        //        request.Task.ForEach(x =>
        //        {
        //            Task t = new Task
        //            {
        //                TaskName = x.TaskName,
        //                TaskDescription = x.TaskDescription,
        //                NumOfVulRequired = x.NumOfVulRequired,
        //                Confirmation = x.Confirmation,
        //                CityCode = x.CityCode,
        //                Lat = x.Lat,
        //                Lng = x.Lng,
        //                TaskHour = x.TaskHour,
        //                RequestCode = req.RequestCode,
        //            };

        //            db.Task.Add(t);
        //            db.SaveChanges();

        //            x.TypesList.ForEach(q =>
        //            {
        //                db.TaskTypes.Add(new TaskTypes
        //                {
        //                    TaskNumber = t.TaskNumber,
        //                    VolunteerCode = db.VolunteerType.FirstOrDefault(y => y.VolunteerName == q).VolunteerCode
        //                });

        //            }
        //            );

        //            x.DatesForTask.ForEach(date =>
        //            {
        //                if (db.Dates.SingleOrDefault(z => z.Date == date) == null)
        //                {
        //                    db.Dates.Add(new Dates
        //                    {
        //                        Date = date
        //                    });
        //                    db.SaveChanges();
        //                }
        //                db.TaskInDates.Add(new TaskInDates
        //                {
        //                    TaskNumber = t.TaskNumber,
        //                    TaskDate = date
        //                });

        //            });

        //        });

        //        db.SaveChanges();

        //        return Ok(new { Status = "OK", Link = link });

        //    }
        //    catch (Exception e)
        //    {
        //        return Content(HttpStatusCode.BadRequest, e.GetType());
        //    }
        //}
    }
}
