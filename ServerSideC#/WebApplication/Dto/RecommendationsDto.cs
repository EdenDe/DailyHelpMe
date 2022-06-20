using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.Dto
{
    public class RecommendationsDto
    {       
        public string Rating { get; set; }
        public string Recommendation { get; set; }
        public DateTime? RatingTime { get; set; }
        public string RaterFirstName { get; set; }
        public string RaterPhoto { get; set; }

        public int TaskRegisteredNum { get; set; }
    }
}