using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication.Models;

namespace WebApplication.Dto
{
    public class EditRequest
    {
        public Requests Request { get; set; }

        public List<Tasks> NewTasks { get; set; }

        public List<Tasks> TaskToRemove { get; set; }
    }
}