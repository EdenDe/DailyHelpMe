using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication.Models;

namespace WebApplication.Dto
{
    public class VolAndTask
    {

        public int TaskNumber { set; get; }
        public string TaskName { set; get; }
        public TimeSpan TaskHour { set; get; }

        public List<DateTime> TaskDates { set; get; }
        public List<UsersPerDate> UsersRegistered { set; get; }
    }
}