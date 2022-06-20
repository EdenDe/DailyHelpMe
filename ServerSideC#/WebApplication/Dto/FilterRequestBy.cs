using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.Dto
{
    public class FilterRequestBy
    {
        public string Type { get; set; }
        public string CityName { set; get; }

        public string Link { set; get; }
        public Location Coords { set; get; }
        public string VolunteerName { get; set; } = "כל התחומים";
    }
}