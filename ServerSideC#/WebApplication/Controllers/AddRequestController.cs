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

                request.Task.ForEach(task =>
                {
                    Task tasks = new Task();
                    if (!task.New)
                    {
                        tasks = req.Task.FirstOrDefault(y => y.TaskNumber == task.TaskNumber);
                    }
                    
                    tasks.TaskName = task.TaskName;
                    tasks.TaskDescription = task.TaskDescription;
                    tasks.NumOfVulRequired = task.NumOfVulRequired;
                    tasks.Confirmation = task.Confirmation;
                    tasks.CityCode = task.CityCode;
                    tasks.Lat = task.Lat;
                    tasks.Lng = task.Lng;
                    tasks.TaskHour = task.TaskHour;
                    tasks.RequestCode = req.RequestCode;

                    if (task.New)
                    {
                        db.Task.Add(tasks);
                    }

                    db.SaveChanges();

                    List<string> typesForTask = new List<string>();
                    List<string> typesToRemoveTask = new List<string>();
                    List<string> typesToAdd = new List<string>();

                    tasks.TaskTypes.ToList().ForEach(type => {
                        if (!task.TypesList.Contains(type.VolunteerType.VolunteerName)) {
                            typesToAdd.Add(type.VolunteerType.VolunteerName);
                        }                   
                    });


                    task.TypesList.ForEach(type => {
                       tasks.TaskTypes.Any(x=> x.VolunteerType.VolunteerName != type)


                    });


                    if (task.New)
                    {
                        task.TypesList.ForEach(q =>
                        {
                            db.TaskTypes.Add(new TaskTypes
                            {
                                TaskNumber = tasks.TaskNumber,
                                VolunteerCode = db.VolunteerType.FirstOrDefault(y => y.VolunteerName == q).VolunteerCode
                            });

                        }
                        );

                        task.DatesForTask.ForEach(date =>
                        {
                            if (db.Dates.SingleOrDefault(z => z.Date == date) == null)
                            {
                                db.Dates.Add(new Dates
                                {
                                    Date = date
                                });
                                db.SaveChanges();
                            }
                            db.TaskInDates.Add(new TaskInDates
                            {
                                TaskNumber = tasks.TaskNumber,
                                TaskDate = date
                            });

                        });
                    }
                  

                });

                db.SaveChanges();

                return Ok(new { Status = "OK", Link = link });

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
