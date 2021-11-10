using Firebase.Storage;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private static readonly IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "8Qcxfs4Nx3SwBX9iLWXKtDRyQ2DHZCBATJD075aF",
            BasePath = "https://aspdata-8d746-default-rtdb.europe-west1.firebasedatabase.app/"
        };
        private static IFirebaseClient client;
        private static readonly string ApiKey = "AIzaSyCxf2rABg_dosQjVmNMh5-XJodMOU0_G04";
        private static readonly string Bucket = "aspdata-8d746.appspot.com";

        // GET: Student
        public ActionResult Index()
        {
            client = new FireSharp.FirebaseClient(config);
            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            string[] roleList = { "Marketing Coordinator" };
            List<SignUpModel> list = new List<SignUpModel>();
            foreach (string role in roleList)
            {
                FirebaseResponse response = client.Get("Account/" + role);
                dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
                if (data != null)
                {
                    foreach (dynamic item in data)
                    {
                        list.Add(JsonConvert.DeserializeObject<SignUpModel>(((JProperty)item).Value.ToString()));
                    }
                }
            }

            List<Course> courses = new List<Course>();
            foreach (SignUpModel id in list)
            {

                FirebaseResponse response = client.Get("Course/" + id.id);
                if (response.Body != "null")
                {
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
                    dynamic json = string.Format("[{0}]", data);
                    Course[] editorial = JsonConvert.DeserializeObject<Course[]>(json);
                    if (editorial[0].Student != null)
                    {
                        if (editorial[0].Student.Contains(sid))
                        {
                            courses.Add(editorial[0]);
                            break;
                        }
                    }

                }
            }


            Dictionary<string, List<string>> mk = new Dictionary<string, List<string>>();
            foreach (Course item in courses)
            {
                FirebaseResponse response = client.Get("Mark/" + item.Coordinator + "/" + sid);
                string mark = JsonConvert.DeserializeObject<string>(response.Body);

                FirebaseResponse responseComment = client.Get("Comment/" + item.Coordinator + "/" + sid);
                string Comment = JsonConvert.DeserializeObject<string>(responseComment.Body);



                List<string> ma = new List<string>
                {
                    JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + item.Coordinator + "/Email").Body),
                    mark,
                    Comment
                };
                mk.Add(item.Coordinator, ma);

            }

            ViewData["comment"] = mk;
            return View(courses);
        }

        public async Task<ActionResult> Submit(string coordinator)
        {
            client = new FireSharp.FirebaseClient(config);
            if (coordinator == null)
            {
                return RedirectToAction("index");
            }
            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            string token = prinicpal.Claims.Where(c => c.Type == "Token").Select(c => c.Value).SingleOrDefault();
            FirebaseResponse response = client.Get("Link/" + coordinator + "/" + sid + "/");
            List<string> nameFile = new List<string>();
            List<string> link = new List<string>();
            if (response.Body != "null")
            {

                List<string> a = JsonConvert.DeserializeObject<List<string>>(response.Body);

                foreach (string item in a)
                {

                    string task = await new FirebaseStorage(Bucket, new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(token),
                        ThrowOnCancel = true
                    }).Child("Student submit").Child(coordinator).Child(sid).Child(item).GetDownloadUrlAsync();
                    link.Add(task);
                    nameFile.Add(item);



                }

            }
            response = client.Get("Course/" + coordinator);

            DateTime dateEnd = JsonConvert.DeserializeObject<Course>(response.Body).dateEnd;
            DateTime dateFinal = JsonConvert.DeserializeObject<Course>(response.Body).dateFinal;
            DateTime dateNow = DateTime.Now;
            if (dateEnd > dateNow)
            {
                ViewData["canSubmit"] = true;
            }
            else
            {
                ViewData["canSubmit"] = false;

            }
            response = client.Get("Link/" + coordinator + "/student");
            List<string> student = JsonConvert.DeserializeObject<List<string>>(response.Body);

            if (student != null)
            {
                if (dateFinal > dateNow && student.IndexOf(sid) != -1)
                {
                    ViewData["canSubmit"] = true;
                }
            }
            ViewData["mail"] = JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + coordinator + "/Email").Body);

            ViewData["dateEnd"] = dateEnd;
            ViewData["nameFile"] = nameFile;
            ViewData["link"] = link;
            ViewData["coordinator"] = coordinator;






            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Submit(Submissions document, HttpPostedFileBase file, string coordinator)
        {
            try
            {
                FileStream stream;
                if (file.ContentLength > 0)
                {
                    client = new FireSharp.FirebaseClient(config);
                    ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
                    string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
                    string path = Path.Combine(Server.MapPath("~/Content/images/"), file.FileName);
                    file.SaveAs(path);
                    stream = new FileStream(Path.Combine(path), FileMode.Open);


                    //await Task.Run(() => up(stream, file.FileName, document.Token, coordinator));
                    List<string> names = new List<string>();


                    FirebaseResponse response = client.Get("Link/" + coordinator + "/" + sid);
                    string name = stream.Name.Split(new string[] { "\\" }, StringSplitOptions.None).Last();
                    if (response.Body != "null")
                    {
                        string[] a = JsonConvert.DeserializeObject<string[]>(response.Body);

                        foreach (string item in a)
                        {
                            names.Add(item);
                            if (item == name)
                            {
                                await new FirebaseStorage(Bucket, new FirebaseStorageOptions
                                {
                                    AuthTokenAsyncFactory = () => Task.FromResult(document.Token),
                                    ThrowOnCancel = true
                                }).Child("Student submit").Child(coordinator).Child(sid).Child(name).DeleteAsync();
                                names.Remove(name);
                            }
                        }
                    }

                    names.Add(name);
                    await client.SetAsync("Link/" + coordinator + "/" + sid, names);
                    await new FirebaseStorage(Bucket, new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(document.Token),
                        ThrowOnCancel = true
                    }).Child("Student submit").Child(coordinator).Child(sid).Child(name).PutAsync(stream);






                    List<string> student = new List<string>();

                    FirebaseResponse response1 = client.Get("Link/" + coordinator + "/" + "/student");

                    if (response1.Body != "null")
                    {
                        List<string> a = JsonConvert.DeserializeObject<List<string>>(response1.Body);
                        foreach (string item in a)
                        {
                            student.Add(item);
                            if (item == sid)
                            {
                                student.Remove(sid);
                            }
                        }
                    }

                    student.Add(sid);
                    //System.Threading.Thread.Sleep(10000);
                    await client.SetAsync("Link/" + coordinator + "/" + "/student", student);

                    string body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
                    MailMessage message = new MailMessage();
                    message.To.Add(new MailAddress(JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + coordinator + "/Email/").Body)));
                    message.From = new MailAddress("thuyenprovjp@outlook.com.vn");  // replace with valid value
                    message.Subject = JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + coordinator + "/Email/").Body) + "has given submitions to get feedback";
                    message.Body = string.Format(body, "Donotreply", "thuyenprovjp@outlook.com.vn", "Student " + sid + " submited");
                    message.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient())
                    {
                        NetworkCredential credential = new NetworkCredential
                        {
                            UserName = "thuyenprovjp@outlook.com.vn",
                            Password = "provjp112233"
                        };
                        smtp.Credentials = credential;
                        smtp.Host = "smtp-mail.outlook.com";
                        smtp.Port = 587;
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(message);
                    }

                    //SmtpClient client1 = new SmtpClient();

                    //client1.Credentials = new NetworkCredential("bphamngocbao@outlook.com.vn", "Crt112233");
                    //client1.UseDefaultCredentials = false;
                    //client1.Port = 587;
                    //client1.Host = "smtp-mail.outlook.com";
                    //client1.EnableSsl = true;

                    //try
                    //{
                    //    MailAddress
                    //        maFrom = new MailAddress("exitlag1m1@gmail.com", "Student Name", Encoding.UTF8),
                    //        maTo = new MailAddress(JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + coordinator + "/Email/").Body), JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + coordinator + "/Name/").Body), Encoding.UTF8);
                    //    MailMessage mmsg = new MailMessage(maFrom.Address, maTo.Address);
                    //    mmsg.Body = "<html><body><h1>Student " + sid + " Submit</h1></body></html>";
                    //    mmsg.BodyEncoding = Encoding.UTF8;
                    //    mmsg.IsBodyHtml = true;
                    //    mmsg.Subject = "Mark";
                    //    mmsg.SubjectEncoding = Encoding.UTF8;

                    //    client1.Send(mmsg);
                    //    MessageBox.Show("Done");
                    //}
                    //catch (Exception ex)
                    //{
                    //    MessageBox.Show(ex.ToString(), ex.Message);
                    //}




                    //var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
                    //var message = new MailMessage();
                    //message.To.Add(JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + coordinator + "/Email/").Body));
                    //message.From = new MailAddress("exitlag1m1@gmail.com");
                    //message.Subject = "Student " + sid + " submit";
                    //string Body = "Student " + sid + " submit";
                    //message.Body = Body;
                    //message.IsBodyHtml = true;
                    //SmtpClient smtp = new SmtpClient();

                    //smtp.Host = "smtp.gmail.com";
                    //smtp.EnableSsl = true;
                    //smtp.Port = 587;
                    ////smtp.Credentials = new System.Net.NetworkCredential("exitlag1m1@gmail.com", "Crt112233"); // Enter seders User name and password  
                    //smtp.UseDefaultCredentials = false;
                    //smtp.Credentials = new System.Net.NetworkCredential("exitlag1m1@gmail.com", "Crt112233");
                    // smtp.Send(message);

                    //using (var smtp = new SmtpClient())
                    //{
                    //    //await smtp.SendMailAsync(message);


                    //}


                    //var b = client.Get("Notifications").Body;
                    //var mail = JsonConvert.DeserializeObject<Dictionary<string, string>>(b);


                    //var senderEmail = new MailAddress(get(mail["Mail"]), "Student submit");
                    //var d = client.Get("Account/Marketing Coordinator/" + coordinator + "/Email/").Body;
                    //var receiverEmail = new MailAddress(JsonConvert.DeserializeObject<string>(d));
                    //var password = get(mail["pass"]);
                    //var sub = "Student " + sid + " submit";
                    //var body = "Student " + sid + " submit";
                    //var smtp = new SmtpClient
                    //{
                    //    Host = "smtp.gmail.com",
                    //    Port = 587,
                    //    EnableSsl = true,
                    //    DeliveryMethod = SmtpDeliveryMethod.Network,
                    //    UseDefaultCredentials = false,
                    //    Credentials = new NetworkCredential(senderEmail.Address, password)
                    //};
                    //using (var mess = new MailMessage(senderEmail, receiverEmail)
                    //{
                    //    Subject = sub,
                    //    Body = body,
                    //    Priority = MailPriority.High
                    //})
                    //{
                    //    smtp.Send(mess);
                    //}

                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                System.Diagnostics.Debug.WriteLine(ex);

            }

            //try
            //{
            //    MailMessage mail = new MailMessage();
            //    mail.To.Add(JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + coordinator + "/Email/").Body));
            //    mail.From = new MailAddress("exitlag1m1@gmail.com");
            //    mail.Subject = "Student " + sid + " submit";
            //    string Body = "Student " + sid + " submit";
            //    mail.Body = Body;
            //    mail.IsBodyHtml = true;
            //    SmtpClient smtp = new SmtpClient();
            //    smtp.Host = "smtp.gmail.com";
            //    smtp.Port = 587;
            //    smtp.UseDefaultCredentials = false;


            //    smtp.Credentials = new System.Net.NetworkCredential("exitlag1m1@gmail.com", "Crt112233"); // Enter seders User name and password  
            //    smtp.EnableSsl = true;
            //    smtp.Send(mail);
            //}
            //catch (Exception ex)
            //{
            //    ModelState.AddModelError(string.Empty, ex.Message);
            //    return View();
            //}
            //var body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
            //var message = new MailMessage();
            //message.To.Add(JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + coordinator + "/Email/").Body)); //replace with valid value
            //message.Subject = "Student Submit";
            //message.Body = string.Format(body, "Student Submit", "exitlag1m1@gmail.com", "Student " + sid + " submit");
            //message.IsBodyHtml = true;
            //using (var smtp = new SmtpClient())
            //{
            //    await smtp.SendMailAsync(message);

            //}



            return RedirectToAction("submit", "student", new { coordinator = coordinator });
        }
        public async void up(FileStream stream, string fileName, string token, string coordinator)
        {
            client = new FireSharp.FirebaseClient(config);
            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();



            List<string> names = new List<string>();


            FirebaseResponse response = client.Get("Link/" + coordinator + "/" + sid);
            string name = stream.Name.Split(new string[] { "\\" }, StringSplitOptions.None).Last();
            if (response.Body != "null")
            {
                string[] a = JsonConvert.DeserializeObject<string[]>(response.Body);

                foreach (string item in a)
                {
                    names.Add(item);
                    if (item == name)
                    {
                        await new FirebaseStorage(Bucket, new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(token),
                            ThrowOnCancel = true
                        }).Child("Student submit").Child(coordinator).Child(sid).Child(name).DeleteAsync();
                    }
                }
            }
            names.Remove(name);
            names.Add(name);
            SetResponse setResponse = client.Set("Link/" + coordinator + "/" + sid, names);
            FirebaseStorageTask task = new FirebaseStorage(Bucket, new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(token),
                ThrowOnCancel = true
            }).Child("Student submit").Child(coordinator).Child(sid).Child(name).PutAsync(stream);


            task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

            string downloadUrl = await task;


        }
        private static string get(string toDecrypt)
        {
            string key = "bimat";

            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return UTF8Encoding.UTF8.GetString(resultArray);

        }
        public async Task<ActionResult> Delete(string coordinator, string name, string token, string i)
        {
            client = new FireSharp.FirebaseClient(config);
            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            await new FirebaseStorage(Bucket, new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(token),
                ThrowOnCancel = true
            }).Child("Student submit").Child(coordinator).Child(sid).Child(name).DeleteAsync();

            FirebaseResponse a = client.Get("Link/" + coordinator + "/" + sid);
            List<string> b = JsonConvert.DeserializeObject<List<string>>(a.Body);

            b.Remove(name);

            await client.SetAsync("Link/" + coordinator + "/" + sid, b);

            if (b.Count == 0)
            {
                List<string> fi = JsonConvert.DeserializeObject<List<string>>(client.Get("Link/" + coordinator + "/student").Body);
                fi.Remove(sid);
                await client.SetAsync("Link/" + coordinator + "/student", fi);
            }

            return RedirectToAction("submit", "student", new { coordinator = coordinator });
        }
        //public async void preview()
        //{
        //    client = new FireSharp.FirebaseClient(config);
        //    var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
        //    var sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

        //    List<string> path = new List<string>();

        //    string fileName = "D:\\4 over\\WebApplication3\\WebApplication1\\WebApplication1\\Content\\images";
        //    WebClient myWebClient = new WebClient();
        //    myWebClient.DownloadFile(path, fileName);


        //    System.IO.File.Delete(path);

        //}

    }
}
