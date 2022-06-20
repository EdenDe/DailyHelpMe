using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.Dto
{
    public class requestlist
    {
        public string RequestName { get; set; }
        public bool? PrivateRequest { get; set; }

        public string TaskName { get; set; }
        public TimeSpan TaskHour { get; set; }

        public string TaskPlace { get; set; }
        public string MobilePhone { get; set; }
        public int TaskNumber { get; set; }
        public bool Past { get; set; }

        public string Status { get; set; }

        public string UserUpload { get; set; }
    }
}