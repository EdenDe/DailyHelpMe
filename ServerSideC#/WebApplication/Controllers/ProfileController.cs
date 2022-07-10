using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApplication.Dto;
using DailyHelpMe;
using WebApplication.Models;
using WebApplication.Controllers;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    public class ProfileController : ApiController
    {
        [Route("futureTaskToDo")]
        [HttpPost]
        public IHttpActionResult Post([FromBody] string id)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                DateTime currentDateTime = DateTime.Now;
                List<RegisterProfile> taskList = new List<RegisterProfile>();

                List<TaskInDates> list = db.RegisteredTo.Where(x => x.ID == id && x.RegisterStatus == "טרם בוצע").Select(t => t.TaskInDates).ToList();
                List<int> TaskNumberList = list.Select(x => x.TaskNumber).Distinct().ToList();

                foreach (var taskNumber in TaskNumberList)
                {
                    List<DateTime> datess = list.Where(y => y.TaskNumber == taskNumber).Select(y => y.TaskDate).ToList();
                    Task task = db.Task.FirstOrDefault(q => q.TaskNumber == taskNumber);

                    taskList.Add(new RegisterProfile
                    {
                        UserUpload = task.Request.Users.FirstName,
                        TaskNumber = task.TaskNumber,
                        TaskName = task.TaskName,
                        TaskHour = task.TaskHour,
                        TaskPlace = task.City.CityName,
                        TaskDates = datess,
                        MobilePhone = task.Request.Users.MobilePhone,
                        Photo = task.Request.Users.Photo,
                    });

                };
                return Ok(taskList);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [Route("taskDoneByMe")]
        [HttpPost]
        public IHttpActionResult TaskDoneByMe([FromBody] string id)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                DateTime currentDateTime = DateTime.Now;
                List<RegisterProfile> taskList = new List<RegisterProfile>();


                List<TaskInDates> list = db.RegisteredTo.Where(x => x.ID == id && x.RegisterStatus == "בוצע").Select(t => t.TaskInDates).ToList();

                List<int> TaskNumberList = list.Select(x => x.TaskNumber).Distinct().ToList();
                foreach (var taskNumber in TaskNumberList)
                {
                    List<DateTime> datess = list.Where(y => y.TaskNumber == taskNumber).Select(y => y.TaskDate).ToList();

                    Task task = db.Task.FirstOrDefault(q => q.TaskNumber == taskNumber);

                    taskList.Add(new RegisterProfile
                    {
                        UserUpload = task.Request.Users.FirstName,
                        TaskNumber = task.TaskNumber,
                        TaskName = task.TaskName,
                        TaskHour = task.TaskHour,
                        TaskPlace = task.City.CityName,
                        TaskDates = datess.OrderBy(x=> x).ToList(),
                        MobilePhone = task.Request.Users.MobilePhone,
                        Photo = task.Request.Users.Photo,
                    });

                };

                taskList= taskList.OrderByDescending(x => x.TaskDates.Max(y => y)).ToList();

                return Ok(taskList);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }



        [Route("getRecommendation")]
        [HttpPost]
        public IHttpActionResult GetRecommendation([FromBody] string id)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                List<RecommendationsDto> list = new List<RecommendationsDto>();

                Users userChosen = db.Users.FirstOrDefault(user => user.ID == id);
                if (userChosen is null)
                {
                    return Ok("NO");
                };

                list = db.RegisteredTo.Where(x => x.ID == id && x.RatingTime != null).ToList().Select(userInRegi => new RecommendationsDto
                {
                    Rating = userInRegi.Rating,
                    Recommendation = userInRegi.Recommendation,
                    RatingTime = userInRegi.RatingTime,
                    RaterFirstName = userInRegi.TaskInDates.Task.Request.Users.FirstName,
                    RaterPhoto = userInRegi.TaskInDates.Task.Request.Users.Photo,

                }).ToList();

                return Ok(new UserDto
                {
                    FirstName = userChosen.FirstName,
                    LastName = userChosen.LastName,
                    Gender = userChosen.Gender,
                    DateOfBirth = userChosen.DateOfBirth,
                    UserDescription = userChosen.UserDescription,
                    Rank = userChosen.Rank,
                    TotalRate = userChosen.TotalRate,
                    Photo = userChosen.Photo,
                    Recommendations = list,
                    Email = userChosen.Email,
                });

            }
            catch (Exception)
            {
                return NotFound();
            }
        }


        [Route("taskToRate")]
        [HttpPost]
        public IHttpActionResult TaskToRate([FromBody] string id)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                List<Tasks> taskList = db.RegisteredTo.Where(x => x.RegisterStatus == "בוצע").Where(x => x.TaskInDates.Task.Request.ID == id).Where(x => x.Rating == null)
                    .Select(task => new Tasks
                    {
                        TaskNumber = task.TaskInDates.Task.TaskNumber,
                        TaskName = task.TaskInDates.Task.TaskName,
                        TaskHour = task.TaskInDates.Task.TaskHour,

                    }).ToList();

                return Ok(taskList);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }


        [Route("rateUser")]
        [HttpPost]
        public IHttpActionResult RateUser([FromBody] RecommendationsDto reco)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                RegisteredTo taskRegistered = db.RegisteredTo.FirstOrDefault(regi => regi.TaskRegisteredNum == reco.TaskRegisteredNum);
                if (taskRegistered is null)
                {
                    return Ok("NO");
                }

                taskRegistered.Recommendation = reco.Recommendation;
                taskRegistered.Rating = reco.Rating;
                taskRegistered.RatingTime = DateTime.Now;

                db.SaveChanges();

                if (reco.Rating != null)
                {
                    List<int> listInt = new List<int>();

                    db.RegisteredTo.Where(x => x.ID == taskRegistered.ID && x.Rating != null).ToList().ForEach(x =>
                        listInt.Add(int.Parse(x.Rating))
                    );

                    taskRegistered.Users.TotalRate = (float)listInt.Average();
                    db.SaveChanges();
                }

                return Ok(taskRegistered.Users.TotalRate);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [Route("changeUserInfo")]
        [HttpPost]
        public IHttpActionResult ChangeUserInfo([FromBody] UserDto userInfo)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                Users user = db.Users.FirstOrDefault(x => x.ID == userInfo.ID);

                if (user is null)
                {
                    return Ok("NO");
                }

                user.UserDescription = userInfo.UserDescription;
                user.FirstName = userInfo.FirstName;
                user.LastName = userInfo.LastName;
                user.MobilePhone = userInfo.MobilePhone;
                user.CityCode = db.City.FirstOrDefault(city => city.CityName == userInfo.CityName).CityCode;
                user.Photo = userInfo.Photo;

                db.SaveChanges();

                return Ok("OK");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        //task done for me!
        [Route("getDoneTasks")]
        [HttpPost]
        public IHttpActionResult GetDoneTasks([FromBody] string id)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                List<RegisteredTo> temp = db.RegisteredTo.Where(regi => regi.TaskInDates.Task.Request.ID == id && regi.RegisterStatus == "בוצע" && regi.RatingTime == null).ToList();

                if (temp != null)
                {
                    return Ok(temp.Select(x => new
                    {
                        x.Users.FirstName,
                        x.Users.LastName,
                        x.TaskInDates.Task.TaskName,
                        x.TaskInDates.TaskDate,
                        x.TaskRegisteredNum,
                        x.Users.Photo,
                        x.Users.Rank,

                    }).ToList());
                }
                return Ok("No");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [Route("changeTaskStatus")]
        [HttpPost]
        public IHttpActionResult ChangeStatus([FromBody] TaskNumDate taskDet)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                RegisteredTo temp = db.RegisteredTo.Where(x => x.ID == taskDet.ID && x.TaskInDates.TaskNumber == taskDet.TaskNumber)
                    .Where(x => x.RegisterStatus == "טרם בוצע").FirstOrDefault();

                if (temp != null)
                {
                    temp.RegisterStatus = "בוצע";
                    db.SaveChanges();
                    return Ok("Ok");
                }
                return Ok("No");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [Route("pastTask")]
        [HttpPost]
        public IHttpActionResult PastTask([FromBody] string id)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                DateTime currentDateTime = DateTime.Now;
                List<RegisterProfile> newTaskList = new List<RegisterProfile>();
                List<TaskInDates> taskList = db.RegisteredTo.Where(x => x.ID == id && x.RegisterStatus == "בוצע").Select(t => t.TaskInDates).ToList();

                List<int> TaskNumberList = taskList.Select(x => x.TaskNumber).Distinct().ToList();
                foreach (int x in TaskNumberList)
                {
                    List<DateTime> dateList = taskList.Where(y => y.TaskNumber == x).Select(y => y.TaskDate).ToList();

                    Task task = db.Task.FirstOrDefault(q => q.TaskNumber == x);

                    newTaskList.Add(new RegisterProfile
                    {
                        UserUpload = task.Request.ID,
                        TaskNumber = task.TaskNumber,
                        TaskName = task.TaskName,
                        TaskHour = task.TaskHour,
                        TaskPlace = task.City.CityName,
                        TaskDates = dateList,
                        MobilePhone = task.Request.Users.MobilePhone,
                    });

                };
                return Ok(newTaskList);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }


        string SetStatus(int taskDateNum)
        {
            DailyHelpMeDbContext db = new DailyHelpMeDbContext();

            if (db.InterestedInRegistered.FirstOrDefault(x => taskDateNum == x.TaskDateNum) != null)
            {
                return "wait";
            }
            else if (db.RegisteredTo.FirstOrDefault(x => taskDateNum == x.TaskDateNum) != null)
            {
                return "signed";
            }
            return "open";

        }

        [Route("blockORActiveUser")]
        [HttpPost]
        public IHttpActionResult BlockUser([FromBody] string id)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                Users u = db.Users.FirstOrDefault(x => x.ID == id);
                if (u is null)
                {
                    return Ok("NO");
                }
                if (u.UStatus == "פעיל")
                {
                    u.UStatus = "חסום";
                }
                else
                {
                    u.UStatus = "פעיל";
                }
                db.SaveChanges();

                return Ok(new string[3] { u.FirstName + " " + u.LastName, u.UStatus, u.ID });
            }
            catch (Exception)
            {
                return Ok("NO");
            }
        }

        [Route("getTheTop3")]
        [HttpPost]
        public IHttpActionResult GetTheTop3()
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                return Ok(db.RegisteredTo.Where(x => x.TaskInDates.TaskDate.Month == DateTime.Today.Month && x.TaskInDates.TaskDate.Year == DateTime.Today.Year && x.RegisterStatus == "בוצע")
                      .GroupBy(x => x.ID).Select(x => new { ID = x.Key, TotalTasks = x.Count() }).OrderByDescending(y => y.TotalTasks).Take(3).ToList().Select(x => new
                      {
                          HowMany = x.TotalTasks,
                          db.Users.FirstOrDefault(y => y.ID == x.ID).FirstName,
                          db.Users.FirstOrDefault(y => y.ID == x.ID).Photo,
                      }).ToList());
            }
            catch (Exception)
            {
                return Ok("NO");
            }
        }

        [Route("managerData")]
        [HttpPost]
        public IHttpActionResult ManagerData([FromBody] string id)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                List<int> taskDatesList = db.RegisteredTo.Select(x => x.TaskDateNum).ToList();

                List<Users> users = db.Users.Where(x => x.ID != id).ToList();
                List<VolunteerType> types = db.VolunteerType.ToList();
                string[,] userList = new string[users.Count(), 3];
                string[,] typesList = new string[types.Count(), 3];

                for (int i = 0; i < userList.GetLength(0); i++)
                {
                    userList[i, 0] = users[i].FirstName + " " + users[i].LastName;
                    userList[i, 1] = users[i].UStatus;
                    userList[i, 2] = users[i].ID;
                }

                for (int i = 0; i < typesList.GetLength(0); i++)
                {
                    typesList[i, 0] = types[i].VolunteerName;
                    typesList[i, 1] = (bool)types[i].Aprroved ? "מאושר" : "לא מאושר";
                    typesList[i, 2] = types[i].VolunteerCode.ToString();
                }

                DataForManager data = new DataForManager
                {
                    TasksDone = db.RegisteredTo.Where(x => x.RegisterStatus == "בוצע").ToList().Count(),
                    CurrentOpenTasks = db.TaskInDates.Select(x => taskDatesList.Contains(x.TaskDateNum)).ToList().Count(),
                    TypeList = typesList,
                    UserList = userList,
                };

                return Ok(data);
            }
            catch (Exception)
            {
                return Ok("NO");
            }
        }

        [Route("myRequests")]
        [HttpPost]
        public IHttpActionResult MyRequests([FromBody] string id)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                List<Request> requestListTry = new List<Request>();

                requestListTry = db.Request.Where(request => request.ID == id && request.RequestStatus == "פעיל").ToList();

                return Ok(MakeRequestsList(requestListTry));
            }
            catch (Exception)
            {
                return Ok("NO");
            }
        }

        public List<Requests> MakeRequestsList(List<Request> requestListTry)
        {

            DailyHelpMeDbContext db = new DailyHelpMeDbContext();

            List<Requests> requestList = new List<Requests>();

            foreach (var request in requestListTry)
            {
                requestList.Add(new Requests
                {
                    RequestCode = request.RequestCode,
                    RequestNew = false,
                    RequestName = request.RequestName,
                    PrivateRequest = request.PrivateRequest,
                    Link = request.Link,
                    RequestStatus = request.RequestStatus,
                    Task = request.Task.Select(task => new Tasks
                    {
                        TaskNumber = task.TaskNumber,
                        New = false,
                        TaskDescription = task.TaskDescription,
                        TaskName = task.TaskName,
                        DatesForTask = task.TaskInDates.Where(taskDate => taskDate.TaskNumber == task.TaskNumber).Select(date => date.TaskDate).OrderBy(date => date).ToList(),
                        TaskHour = task.TaskHour,
                        NumOfVulRequired = task.NumOfVulRequired,
                        TypesList = task.TaskTypes.Where(taskType => taskType.TaskNumber == task.TaskNumber).Select(type => type.VolunteerType.VolunteerName).ToList(),
                        CityName = task.City.CityName,
                        CityCode = task.CityCode,
                        Lat = task.Lat,
                        Lng = task.Lng,
                        Confirmation = task.Confirmation,
                        TaskDateStatus = task.TaskInDates.Select(taskDate => new TaskStatus
                        {
                            TaskDateNum = taskDate.TaskDateNum,
                            TaskDate = taskDate.TaskDate,
                            Status = SetStatus(taskDate.TaskDateNum),
                            UserSigned = SetUsers(taskDate.TaskDateNum)
                        }).ToList(),

                    }).ToList(),
                    EndDate = db.Task.Where(x => x.RequestCode == request.RequestCode).Max(x => x.TaskInDates.Max(y => y.TaskDate)),
                    StartDate = db.Task.Where(x => x.RequestCode == request.RequestCode).Min(x => x.TaskInDates.Min(y => y.TaskDate)),

                });
            }

            return requestList;
        }

        List<UserDto> SetUsers(int taskDateNum)
        {
            DailyHelpMeDbContext db = new DailyHelpMeDbContext();
            List<Users> usersList = new List<Users>();
            List<UserDto> usersDtoList = new List<UserDto>();

            usersDtoList = db.RegisteredTo.Where(regi => regi.TaskDateNum == taskDateNum)
                .Select(task => new UserDto
                {
                    FirstName = task.Users.FirstName,
                    LastName = task.Users.LastName,
                    MobilePhone = task.Users.MobilePhone,
                    TotalRate = task.Users.TotalRate,
                    Photo = task.Users.Photo,
                    ID = task.Users.ID,
                    Status = "signed",
                }).ToList();


            db.InterestedInRegistered.Where(regi => regi.TaskDateNum == taskDateNum).Select(task => task.Users).ToList().ForEach(user =>
            {
                usersDtoList.Add(new UserDto
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    MobilePhone = user.MobilePhone,
                    TotalRate = user.TotalRate,
                    ID = user.ID,
                    Photo = user.Photo,
                    Status = "wait",
                });
            });

            return usersDtoList;
        }


        [Route("pastRequest")]
        [HttpPost]
        public IHttpActionResult GetPastRequest([FromBody] string id)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                List<Request> requestListTry = new List<Request>();

                requestListTry = db.Request.Where(request => request.ID == id && request.RequestStatus == "עבר").ToList();

                return Ok(MakeRequestsList(requestListTry));
            }
            catch (Exception)
            {
                return Ok("NO");
            }
        }



        [Route("deleteRequest")]
        [HttpDelete]
        public IHttpActionResult DeleteRequest([FromBody] int requestCode)
        {
            try
            {
                PushNotificationsController push = new PushNotificationsController();
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                Request request = new Request();

                request = db.Request.FirstOrDefault(req => req.RequestCode == requestCode);

                if (request is null)
                {
                    return Ok("NO");
                }

                foreach (Task task in request.Task)
                {
                    List<TaskInDates> taskDates = task.TaskInDates.ToList();

                    db.TaskTypes.RemoveRange(db.TaskTypes.Where(x => x.TaskNumber == task.TaskNumber).ToList());

                    foreach (var taskDate in taskDates)
                    {
                        taskDate.InterestedInRegistered.ToList().ForEach(x =>
                        {
                            push.PushNoti(new PushNoteData
                            {
                                to = x.Users.TokenID,
                                title = "התנדבות בוטלה",
                                body = $"המשימה {x.TaskInDates.Task.TaskName} בוטלה ולכן ההמתנה לשיבוצך נמחקה",
                            });

                            db.InterestedInRegistered.Remove(x);

                        });

                        taskDate.RegisteredTo.ToList().ForEach(x =>
                        {
                            push.PushNoti(new PushNoteData
                            {
                                to = x.Users.TokenID,
                                title = "התנדבות בוטלה",
                                body = $"המשימה {x.TaskInDates.Task.TaskName} בוטלה ולכן שיבוצך נמחק",

                            });

                            db.RegisteredTo.Remove(x);
                        });

                        db.TaskInDates.Remove(taskDate);
                    }
                }

                db.Request.Remove(request);
                db.SaveChanges();

                return Ok("OK");
            }
            catch (Exception)
            {
                return Ok("NO");
            }
        }


    }
}
