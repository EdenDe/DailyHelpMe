using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication.Models;

namespace WebApplication.Dto
{
    public class DateAndTasks
    {
       public DateTime Date { get; set; }

        public List<TaskAndUser> TaskList{ get; set; }
    }
}