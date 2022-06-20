using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.Dto
{
    public class TaskStatus
    {
        public DateTime TaskDate { get; set; }
        public string Status { get; set; }
        public int TaskDateNum { get; set; }
        public List<UserDto> UserSigned { get; set; }

        public int TaskNumber { get; set; }
    }
}