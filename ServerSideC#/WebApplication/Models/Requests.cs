using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DailyHelpMe;

namespace WebApplication.Models
{
    public partial class Requests
    {
        public string UserUpload { get; set; }
        public int RequestCode { get; set; }
        public string RequestName { get; set; }
        public string ID { get; set; }
        public byte? Rank { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Image { get; set; }
        public string RequestStatus { get; set; }
        public string City { get; set; }
        public List<Tasks> Task { get; set; }
        public List<string> TypesList { get; set; }
        public bool? PrivateRequest { get; set; }
        public string Link { get; set; }

        public bool RequestNew { get; set; }


    }
}