using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Web.Http;
using DailyHelpMe;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class TimerController : ApiController
    {
        //code for timer
        [HttpGet]
        [Route("api/start")]
        public void StartTimer()
        {
            WebApiApplication.StartTimer1();
        }

        //code for timer
        [HttpGet]
        [Route("api/stop")]
        public void StopTimer()
        {
            WebApiApplication.EndTimer1();
        }
        
    }
}
