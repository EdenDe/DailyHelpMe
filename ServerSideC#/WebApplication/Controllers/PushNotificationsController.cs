using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DailyHelpMe;
using WebApplication.Dto;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace WebApplication.Controllers
{
    public class PushNotificationsController : ApiController
    {

        [Route("sendpushnotification")]
        [HttpPost]
        public string PushNoti(PushNoteData pnd)
        {
            // Create a request using a URL that can receive a post.   
            WebRequest request = WebRequest.Create("https://exp.host/--/api/v2/push/send");
            // Set the Method property of the request to POST.  
            request.Method = "POST";
            // Create POST data and convert it to a byte array.  
            string postData = new JavaScriptSerializer().Serialize(pnd);
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentType property of the WebRequest.  
            request.ContentType = "application/json";
            // Set the ContentLength property of the WebRequest.  
            request.ContentLength = byteArray.Length;
            // Get the request stream.  
            Stream dataStream = request.GetRequestStream();
            // Write the data to the request stream.  
            dataStream.Write(byteArray, 0, byteArray.Length);
            // Close the Stream object.  
            dataStream.Close();
            // Get the response.  
            WebResponse response = request.GetResponse();
            // Display the status.  
            string returnStatus = ((HttpWebResponse)response).StatusDescription;
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.  
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.  
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.  
            string responseFromServer = reader.ReadToEnd();
            // Display the content.  
            //Console.WriteLine(responseFromServer);
            // Clean up the streams.  
            reader.Close();
            dataStream.Close();
            response.Close();

            return "success:) --- " + responseFromServer + ", " + returnStatus;
        }



        [Route("updateToken")]
        [HttpPost]
        public IHttpActionResult Post([FromBody] AddToken user)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                Users u = db.Users.FirstOrDefault(x => x.ID == user.ID);
                if (u != null)
                {
                    u.TokenID = user.Token;

                    db.SaveChanges();

                    return Ok("Token Saved");
                }
                return NotFound();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}
