using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DailyHelpMe;

namespace WebApplication.Dto
{
    public class UserDto
    {
        public string ID { get; set; }

        public string UserDescription { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobilePhone { get; set; }
        public string Passwords { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Photo { get; set; }
        public string Gender { get; set; }
        public string CityName { get; set; }
        public string TokenID { get; set; }
        public List<string> VolunteerTypes { get; set; }
        public string HowSigned { get; set; }
        public string UStatus { get; set; }
        public int? OpenRequests { get; set; }
        public int? RegisteredTasks { get; set; }
        public int? PastRequests { get; set; }
        public int? TaskDone { get; set; }

        public byte? Rank { get; set; }
        public float? TotalRate { get; set; }
        public string Status { get; set; }

        public List<RecommendationsDto> Recommendations { get; set; }
    }
}