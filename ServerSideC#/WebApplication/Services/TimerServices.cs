using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication.Controllers;
using WebApplication.Dto;
using DailyHelpMe;
using System.Data.Entity;

namespace WebApplication.Services
{
    public static class TimerServices
    {
        public static void CheckIf24Hpassed(string path)
        {
            try
            {
                
                DateTime currentDateTime = DateTime.Now;
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                PushNotificationsController push = new PushNotificationsController();

               List<InterestedInRegistered> regi = db.InterestedInRegistered.Where(x => DbFunctions.AddDays(x.SignToTaskTime, 1) <= currentDateTime).ToList();

                foreach (InterestedInRegistered item in regi)
                {
                    db.InterestedInRegistered.Remove(item);
                    db.SaveChanges();

                    //push.PushNoti(new PushNoteData
                    //{
                    //    to = db.Users.FirstOrDefault(x => x.ID == item.ID).TokenID,
                    //    title = "בקשת ההתנדבות לא אושרה",
                    //    body = $"מעלה הבקשה לא אישר את שיבוצך ב24 שעות האחרונות ולכן ניסיון שיבוצך לבקשה {item.TaskInDates.Task.TaskName} מתבטלת"
                    //});

                    //push.PushNoti(new PushNoteData
                    //{
                    //    to = db.Users.FirstOrDefault(x => x.ID == item.TaskInDates.Task.Request.ID).TokenID,
                    //    title = "בקשת ההתנדבות לא אושרה",
                    //    body = $" ניסיון השיבוץ של {item.Users.FirstName} לבקשתך {item.TaskInDates.Task.TaskName} בוטל עקב אי אישורה ",

                    //});

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

              List<Request> reqList= db.Request.Where(request =>
                    request.Task.Where(task => task.RequestCode == request.RequestCode).Max(x => x.TaskInDates.Max(y => y.TaskDate)) < DateTime.Today).ToList();

                reqList.ForEach(x => {
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
               // int twoMounthAgo = DateTime.Now.Month - 2;
                int oneMonthAgo = DateTime.Now.Month - 1;
                int thisMonth = DateTime.Now.Month;
                int amountVolNow = 0;
                int amountVol1 =0;
                //int amountVol2 = 0;
                int avgAmountOfVol = 0;
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                List<Users> userList = db.Users.ToList();

                userList.ForEach(user => {

                    // amountVol2 = user.RegisteredTo.Where(task => task.TaskInDates.TaskDate.Month == twoMounthAgo).Count();
                    List<RegisteredTo> taskDone = user.RegisteredTo.Where(task => task.TaskInDates.TaskDate.Month == oneMonthAgo).ToList();
                    amountVol1 = taskDone.Count();
                    amountVolNow = user.RegisteredTo.Where(task => task.TaskInDates.TaskDate.Month == thisMonth).Count();
                    amountVolNow += user.InterestedInRegistered.Where(task => task.TaskInDates.TaskDate.Month == thisMonth).Count();
                    //avgAmountOfVol = (amountVol2 + amountVol1) / 2;

                    if (amountVol1 > thisMonth)
                    {
                        user.RegisteredTo.Select(task => task.TaskInDates.Task.TaskTypes.GroupBy(type => type.VolunteerCode).Select(x =>
                          new { ID = user.ID, Type = x.Key, typesOfType = x.Count() }).OrderByDescending(y => y.typesOfType).Take(2)).ToList();                            
                    }
                   //.Select(x => new { ID = x.Key, TotalTasks = x.Count() }).OrderByDescending(y => y.TotalTasks).Take(3).Select(x => x.ID).ToList();

                });

               
                db.SaveChanges();

            }
            catch (Exception)
            {
                return;
            }

        }




    }
}