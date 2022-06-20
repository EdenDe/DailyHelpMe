using DailyHelpMe;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication.Dto;


namespace WebApplication.Models
{
    public partial class Tasks
    {
        public int TaskNumber { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public TimeSpan TaskHour { get; set; }
        public List<TaskStatus> TaskDateStatus { get; set; }    
        public bool? Confirmation { get; set; }
        public int RequestCode { get; set; }
        public string Status { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }
        public string CityName { get; set; }
       public byte NumOfVulRequired { get; set; }
        public int? CityCode { get; set; }

        public List<string> TypesList { get; set; }
        public List<DateTime> DatesForTask { get; set; }

        public bool New { get; set; }
    }
}