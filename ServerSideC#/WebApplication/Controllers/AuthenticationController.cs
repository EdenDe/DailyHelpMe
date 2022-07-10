using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DailyHelpMe;
using WebApplication.Dto;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;

namespace WebApplication.Controllers
{
    public class AuthenticationController : ApiController
    {
        [Route("logIn")]
        [HttpPost]
        public IHttpActionResult Post([FromBody] UserForReg CheckUser)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();

                string password = ComputeSha256Hash(CheckUser.Passwords);

                Users user = db.Users.SingleOrDefault(x => x.Email == CheckUser.Email);
                if (user == null || user.Passwords != password)
                    return Ok("user not exists");
                else if (user.UStatus == "חסום")
                    return Ok("user blocked");
                else
                    return Ok(UserGet(user));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("addUser")]
        [HttpPost]
        public IHttpActionResult Post([FromBody] UserDto user)
        {
            try
            {
 
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                Users u = new Users {
                    ID = user.ID,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    TokenID = user.TokenID,
                    Email = user.Email,
                    UStatus = "פעיל",
                    TotalRate = null,
                    Photo = user.Photo,
                };
                
                if (user.HowSigned != "Google" && user.HowSigned != "Facebook")
                {
                    if (db.City.Select(x => x.CityName == user.CityName).ToList() == null)
                    {
                        db.City.Add(new City { CityName = user.CityName });
                        db.SaveChanges();
                    }

                    u.CityCode = db.City.FirstOrDefault(x => x.CityName == user.CityName).CityCode;
                    u.Passwords = ComputeSha256Hash(user.Passwords);
                    u.MobilePhone = user.MobilePhone;
                    u.DateOfBirth = user.DateOfBirth;
                    u.Gender = user.Gender;
                }

                db.Users.Add(u);

                db.SaveChanges();

                if (user.VolunteerTypes != null)
                {
                    List<string> typeList = user.VolunteerTypes;
                    if (user.VolunteerTypes.Contains("כל התחומים"))
                    {
                        typeList = db.VolunteerType.Select(x => x.VolunteerName).ToList();
                    }

                    foreach (var item in typeList)
                    {
                        db.VolTypesPreferences.Add(new VolTypesPreferences
                        {
                            ID = user.ID,
                            VolunteerCode = db.VolunteerType.FirstOrDefault(x => x.VolunteerName == item).VolunteerCode,
                            Dummy = false,
                        });
                    }

                    db.SaveChanges();
                }
                return Ok("OK");
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, e.GetType());
            }
        }

        UserDto UserGet(Users user) {

            return new UserDto
            {
                ID = user.ID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MobilePhone = user.MobilePhone,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Photo = user.Photo,
                Gender = user.Gender,
                CityName = user.City?.CityName,
                UserDescription = user.UserDescription,
                TotalRate = user.TotalRate,
                Rank = user.Rank,
                TokenID = user.TokenID,
                UStatus = user.UStatus,
                OpenRequests = user.Request.Where(request => request.ID == user.ID && request.RequestStatus == "פעיל").Count(),
                RegisteredTasks = user.RegisteredTo.Where(task => task.ID == user.ID && task.RegisterStatus == "טרם בוצע").Count(),
                PastRequests = user.Request.Where(request => request.ID == user.ID && request.RequestStatus == "עבר").Count(),
                TaskDone = user.RegisteredTo.Where(task => task.ID == user.ID && task.RegisterStatus == "בוצע").Count(),
            };
        }

        [Route("searchUser")]
        [HttpPost]
        public IHttpActionResult Post([FromBody] string token)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                Users user = db.Users.FirstOrDefault(x => x.TokenID == token);
                if (user != null)
                {
                    return Ok(UserGet(user));
                }
                return Ok("NO");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [Route("getUser")]
        [HttpPost]
        public IHttpActionResult GetUser([FromBody] string id)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                Users user = db.Users.FirstOrDefault(x => x.ID == id);
                if (user != null)
                {
                    return Ok(UserGet(user));
                }
                return Ok("NO");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        static string ComputeSha256Hash(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


        [Route("checkIfEmailUsed")]
        [HttpPost]
        public IHttpActionResult CheckIfEmailUsed([FromBody] string Email)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                Users u = db.Users.SingleOrDefault(x => x.Email == Email);

                if (u == null)
                {
                    return Ok(false);
                }
                return Ok("כבר נרשמת עם אימייל זה");
            }
            catch (Exception)
            {
                return NotFound();
            }

        }

        [Route("getUsersNameAndPhoto")]
        [HttpPost]
        public IHttpActionResult CheckPhone([FromBody] List<string> usersEmail)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                List<Users> listUser = new List<Users>();
                usersEmail.ForEach(x =>
                {
                    listUser.Add(db.Users.FirstOrDefault(y => y.Email == x));
                });

                return Ok(listUser);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }


        [Route("checkIfPhoneUsed")]
        [HttpPost]
        public IHttpActionResult CheckPhone([FromBody] string phone)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                Users u = db.Users.SingleOrDefault(x => x.MobilePhone == phone);

                if (u == null)
                {
                    return Ok(false);
                }
                return Ok("מספר זה כבר קיים במערכת");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }


        [Route("checkIfIDValidOrUsed")]
        [HttpPost]
        public IHttpActionResult PostID([FromBody] string id)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                Users uId = db.Users.SingleOrDefault(x => x.ID == id);
                if (uId == null)
                {
                    if (AlgorithemToCheckID(id) % 10 == 0)
                    {
                        return Ok(false);
                    }
                    else
                    {
                        return Ok("תעודת זהות לא תקינה");
                    }
                }
                return Ok("תעודת הזהות כבר קיימת במערכת");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }


        static int AlgorithemToCheckID(string id)
        {
            int sum = 0, integer;

            for (int i = 0; i < id.Length; i++)
            {
                integer = Convert.ToInt32(id[i]) - 48;
                if (i % 2 == 0)
                    sum += integer * 1;
                else
                {
                    integer *= 2;
                    if (integer >= 10)
                        integer = integer % 10 + integer / 10;

                    sum += integer;
                }
            }
            return sum;
        }




        [Route("uploadpicture")]
        public Task<HttpResponseMessage> Post()
        {
            string output = "start---";
            List<string> savedFilePath = new List<string>();
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string rootPath = HttpContext.Current.Server.MapPath("~/uploadFiles");
            var provider = new MultipartFileStreamProvider(rootPath);
            var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }
                    foreach (MultipartFileData item in provider.FileData)
                    {
                        try
                        {
                            output += " ---here";
                            string name = item.Headers.ContentDisposition.FileName.Replace("\"", "");
                            output += " ---here2=" + name;

                            //need the guid because in react native in order to refresh an inamge it has to have a new name
                            string newFileName = Path.GetFileNameWithoutExtension(name) + "_" + CreateDateTimeWithValidChars() + Path.GetExtension(name);
                            //string newFileName = Path.GetFileNameWithoutExtension(name) + "_" + Guid.NewGuid() + Path.GetExtension(name);
                            //string newFileName = name + "" + Guid.NewGuid();
                            output += " ---here3" + newFileName;

                            //delete all files begining with the same name
                            string[] names = Directory.GetFiles(rootPath);
                            foreach (var fileName in names)
                            {
                                if (Path.GetFileNameWithoutExtension(fileName).IndexOf(Path.GetFileNameWithoutExtension(name)) != -1)
                                {
                                    File.Delete(fileName);
                                }
                            }

                            File.Copy(item.LocalFileName, Path.Combine(rootPath, newFileName), true);
                            File.Delete(item.LocalFileName);
                            output += " ---here4";

                            Uri baseuri = new Uri(Request.RequestUri.AbsoluteUri.Replace(Request.RequestUri.PathAndQuery, string.Empty));
                            output += " ---here5";
                            string fileRelativePath = "~/uploadFiles/" + newFileName;
                            output += " ---here6 imageName=" + fileRelativePath;
                            Uri fileFullPath = new Uri(baseuri, VirtualPathUtility.ToAbsolute(fileRelativePath));
                            output += " ---here7" + fileFullPath.ToString();
                            savedFilePath.Add(fileFullPath.ToString());
                        }
                        catch (Exception ex)
                        {
                            output += " ---excption=" + ex.Message;
                            string message = ex.Message;
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.Created, savedFilePath[0] + "!" + provider.FileData.Count + "!" + output);
                });
            return task;
        }

        private string CreateDateTimeWithValidChars()
        {
            return DateTime.Now.ToString().Replace('/', '_').Replace(':', '-').Replace(' ', '_');
        }


        [Route("setNewPassword")]
        [HttpPost]
        public IHttpActionResult GetUserEmail([FromBody] Users user)
        {
            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                Users u = db.Users.SingleOrDefault(x => x.ID == user.ID);

                if (u is null)
                {
                    return Ok("NO");
                }

                string password = ComputeSha256Hash(user.Passwords);

                u.Passwords = password;

                db.SaveChanges();

                return Ok("OK");
            }
            catch (Exception)
            {
                return Ok("NO");
            }

        }

        [Route("sendMail")]
        [HttpPost]
        public IHttpActionResult SendMail([FromBody] string id)
        {

            try
            {
                DailyHelpMeDbContext db = new DailyHelpMeDbContext();
                Users u = db.Users.SingleOrDefault(x => x.ID == id);
                if (u is null) {
                    return Ok("NO");
                }
                string email = u.Email;
                string userName = u.FirstName;

                string code = new Random().Next(1000, 9999).ToString();          

                MailMessage message = new MailMessage(
                         "DailyHelpMeTeam rupb862022@gmail.com",
                          email,
                          "שינוי סיסמא",
                          $"שלום {userName},\nהקוד לשינוי סיסמה הוא: {code} \n\nאם אינך שלחת בקשה לשינוי סיסמה, אנא התעלם ממייל זה."
                          );

                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("rupb862022@gmail.com", "qymscsvqbdjyeyce"),
                    EnableSsl = true
                };

                client.Send(message);

                return Ok(code);

            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }
        }
    }
}
