using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.Dto
{
    public class TaskAndUser
    {
        public int TaskNumber { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public TimeSpan TaskHour { get; set; }
        public string CityName { get; set; }
        public string SignStatus { get; set; }
        public string Photo { get; set; }
        public string FirstName { get; set; }
        public string MobilePhone { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }

    }
}