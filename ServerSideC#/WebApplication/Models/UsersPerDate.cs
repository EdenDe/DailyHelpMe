using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.Models
{
    public class UsersPerDate
    {
        public string FirstName { set; get; }
        public string LastName { set; get; }
        public string ID { set; get; }
        public string Photo { set; get; }
        public DateTime? Date { set; get; }
        public float? TotalRate { get; set; }
        public string Status { set; get; }
        public byte? Rank { get; set; }
    }
}