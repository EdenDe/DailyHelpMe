using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.Dto
{
    public class RegisterProfile
    {
        public string TaskName { get; set; }
        public TimeSpan TaskHour { get; set; }
        public List<DateTime> TaskDates { get; set; }
        public string TaskPlace { get; set; }
        public string MobilePhone { get; set; }
        public int TaskNumber { get; set; }
        public bool Past { get; set; }

        public string Status { get; set; }

        public string UserUpload { get; set; }

    }
}