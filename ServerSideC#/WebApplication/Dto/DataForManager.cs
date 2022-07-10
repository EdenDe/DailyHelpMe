using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DailyHelpMe;

namespace WebApplication.Dto
{
    public class DataForManager
    {
       public int TasksDone { get; set; }

        public int CurrentOpenTasks { get; set; }

        public string[,] TypeList { get; set; }

        public string[,] UserList { get; set; }

    }
}