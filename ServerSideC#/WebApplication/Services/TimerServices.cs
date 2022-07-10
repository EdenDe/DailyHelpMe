using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication.Controllers;
using WebApplication.Dto;
using DailyHelpMe;
using System.Data.Entity;
using WebApplication.Models;
using System.Net.Http;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;

namespace WebApplication.Services
{
    public static class TimerServices
    {
        public static void CheckIf24Hpassed()
        {
            try
            {

                DateTime currentDateTime = DateTime.Now;
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                PushNotificationsController push = new PushNotificationsController();
                List<InterestedInRegistered> regi = db.InterestedInRegistered.Where(x => DbFunctions.AddDays(x.SignToTaskTime, 1) <= currentDateTime).ToList();

                foreach (InterestedInRegistered item in regi)
                {
                    if (item.Users.TokenID != null)
                    {
                        push.PushNoti(new PushNoteData
                        {
                            to = item.Users.TokenID,
                            title = "בקשת ההתנדבות לא אושרה",
                            body = $"מעלה הבקשה לא אישר את שיבוצך ב24 שעות האחרונות ולכן ניסיון שיבוצך לבקשה {item.TaskInDates.Task.TaskName} מתבטלת"
                        });
                    }

                    if (item.TaskInDates.Task.Request.Users.TokenID != null) {
                        push.PushNoti(new PushNoteData
                        {
                            to = item.TaskInDates.Task.Request.Users.TokenID,
                            title = "בקשת ההתנדבות לא אושרה",
                            body = $" ניסיון השיבוץ של {item.Users.FirstName} לבקשתך {item.TaskInDates.Task.TaskName} בוטל עקב אי אישורה ",

                        });
                    }
               
                    db.InterestedInRegistered.Remove(item);
                    db.SaveChanges();

                }

            }
            catch (Exception)
            {
                return;
            }


        }

        public static void CheckTopThree()
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                List<string> IDlist = db.RegisteredTo.Where(x => x.TaskInDates.TaskDate.Month == (DateTime.Today.Month - 1) && x.RegisterStatus == "בוצע")
                      .GroupBy(x => x.ID).Select(x => new { ID = x.Key, TotalTasks = x.Count() }).OrderByDescending(y => y.TotalTasks).Take(3).Select(x => x.ID).ToList();

                List<Users> usersList = db.Users.ToList();

                usersList.ForEach(x => x.Rank = 0);

                byte rank = 1;
                IDlist.ForEach(userID =>
                {
                    usersList.FirstOrDefault(user => user.ID == userID).Rank = rank++;
                });

                db.SaveChanges();

            }
            catch (Exception)
            {
                return;
            }
        }

        public static void CheckIfRequestPasted()
        {
            try
            {

                DateTime currentDateTime = DateTime.Now;
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                List<Request> reqList = db.Request.Where(request =>
                       request.Task.Where(task => task.RequestCode == request.RequestCode).Max(x => x.TaskInDates.Max(y => y.TaskDate)) < DateTime.Today).ToList();

                reqList.ForEach(x =>
                {
                    x.RequestStatus = "עבר";
                });

                db.SaveChanges();

            }
            catch (Exception)
            {
                return;
            }

        }


        public static void CustomerLearner()
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
                        avgAmountOfVol += user.RegisteredTo.Where(task => task.TaskInDates.TaskDate.Month == thisMonth - i).Count();
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
                                 new { Type = x.Key, typesOfType = x.Count() }).OrderByDescending(y => y.typesOfType).Take(2).Select(x => x.Type).ToList();

                        if (topTypeName.Count == 0)
                        {
                            topTypeName.AddRange(user.VolTypesPreferences.Take(2).Select(x => x.VolunteerType.VolunteerName));
                        }
                        if (topTypeName.Count == 0)
                        {
                            continue;
                        }

                        List<Tasks> taskList = new List<Tasks>();
                        db.TaskInDates.Where(task => task.TaskDate > DateTime.Today && task.Task.Request.ID != user.ID).ToList()
                             .Where(x => !x.RegisteredTo.Any())
                             .Where(y => y.Task.TaskTypes.Where(x => x.VolunteerType.VolunteerName == topTypeName[0] || x.VolunteerType.VolunteerName == topTypeName[1])
                             .ToList().Count() != 0).ToList()
                             .ForEach(task =>
                             {
                                 if (taskList.Count > 2) { return; }
                                 Tasks tasky = taskList.FirstOrDefault(x => x.TaskNumber == task.TaskNumber);
                                 if (tasky is null)
                                 {
                                     taskList.Add(new Tasks
                                     {
                                         TaskName = task.Task.TaskName,
                                         TaskNumber = task.TaskNumber,
                                         TaskDescription = task.Task.TaskDescription,
                                         DatesForTask = new List<DateTime> { task.TaskDate },
                                         CityName = task.Task.City.CityName,
                                         TaskHour = task.Task.TaskHour,
                                         TypesList = task.Task.TaskTypes.Select(x => x.VolunteerType.VolunteerName).ToList(),
                                         ReqLink = task.Task.Request.Link
                                     });
                                 }
                                 else
                                 {
                                     if (tasky.DatesForTask.Count < 4)
                                     {
                                         tasky.DatesForTask.Add(task.TaskDate);
                                     }
                                 }
                             });

                        SendEmail(taskList, user);

                    }

                }

            }
            catch (Exception)
            {
                return;
            }

        }


        static void SendEmail(List<Tasks> taskList, Users user)
        {

            try
            {
                string body = ""; int counter = 0;

                body += $"<div style='text-align:center;padding-right:10px;font-size:12px;color:black;font-family:sans-serif;'> <p style='color:black'> שלום {user.FirstName} </p>" +
                         "<p style='color:black'> ראינו שהחודש פעילותך באפליקציה פחתה<br> ולכן אספנו עבורך המלצות למשימות שעשויות לעניין אותך </p>";
                taskList.ForEach(task =>
                {
                    body += $"<p style='font-weight:normal;color:black'> <span style='font-weight:bold;font-size:14px;color:black'> {task.TaskName} </span>  <br>" +
                              $"בשעה: {task.TaskHour.ToString().Substring(0, 5)} \t  בעיר: {task.CityName} <br> בתחומי העיניין: ";
                    counter = 0;
                    task.TypesList.ForEach(type =>
                    {
                        counter++;
                        if (counter == task.TypesList.Count())
                        {
                            body += type + "<br>בתאריכים: ";
                        }
                        else
                        {
                            body += type + ", ";
                        }
                    });
                    counter = 0;
                    task.DatesForTask.ForEach(date =>
                    {
                        counter++;
                        if (counter == task.DatesForTask.Count())
                        {
                            body += date.ToShortDateString() + "<br>"; ;
                        }
                        else
                        {
                            body += date.ToShortDateString() + " | ";
                        }
                    });
                    body += $"הקוד בקשה למציאתה מהר באפליקציה: {task.ReqLink} </p> ";
                });

                body += "<br> <p style='color:black'> תודה רבה על היותך חלק מהקהילה שלנו<br> צוות DailyHelpMe <br>" +
                         "<img style = 'width:200px' src = 'https://proj.ruppin.ac.il/bgroup86/prod/uploadFiles/LogoReg.png'/></p> </div>";

                string email = user.Email;
                string userName = user.FirstName;

                MailMessage message = new MailMessage(
                         "DailyHelpMeTeam rupb862022@gmail.com",
                          email,
                          $"{user.FirstName} ההתאמות עבורך מחכות לך באפליקציה",
                           body
                          )
                {
                    IsBodyHtml = true
                };
                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("rupb862022@gmail.com", "qymscsvqbdjyeyce"),
                    EnableSsl = true
                };

                client.Send(message);

            }
            catch (Exception)
            {

            }
        }




    }
}