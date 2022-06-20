using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace WebApplication.Dto
{
    public class UserAndFilter
    {
        public UserAndFilter()
        {
            List<FilterRequestBy> FilterBy = new List<FilterRequestBy>();
        }
        public string ID { get; set; }
        public List<FilterRequestBy> FilterBy { set; get; } 
 
    }
}