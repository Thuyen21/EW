using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ChatController : Controller

    {
        private static IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "8Qcxfs4Nx3SwBX9iLWXKtDRyQ2DHZCBATJD075aF",
            BasePath = "https://aspdata-8d746-default-rtdb.europe-west1.firebasedatabase.app/"
        };
        private static IFirebaseClient client;
        private static string ApiKey = "AIzaSyCxf2rABg_dosQjVmNMh5-XJodMOU0_G04";
        private static string Bucket = "aspdata-8d746.appspot.com";
        // GET: Chat
        public ActionResult Index()
        {
            var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            var roleNow = prinicpal.Claims.Where(c => c.Type == "Role").Select(c => c.Value).SingleOrDefault();
            client = new FireSharp.FirebaseClient(config);
            var chat = new Dictionary<string, string>();
            string[] roleList = new string[1];
            if (roleNow == "Marketing Coordinator")
            {
                var course = client.Get("Course/" + sid);
                if (course.Body != "null")
                {
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(course.Body);
                    var json = string.Format("[{0}]", data);
                    Course[] editorial = JsonConvert.DeserializeObject<Course[]>(json);
                    if (editorial[0].Student != null)
                    {
                        foreach (var item in editorial[0].Student)
                        {
                            chat.Add(item, JsonConvert.DeserializeObject<string>(client.Get("Account/Student/" + item + "/Email").Body));
                        }
                    }


                }


            }
            else if (roleNow == "Student")
            {
                roleList = new string[] { "Marketing Coordinator" };
                var list = new List<SignUpModel>();
                foreach (string role in roleList)
                {
                    FirebaseResponse response = client.Get("Account/" + role);
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
                    if (data != null)
                        foreach (var item in data)
                        {
                            list.Add(JsonConvert.DeserializeObject<SignUpModel>(((JProperty)item).Value.ToString()));
                        }
                }


                foreach (var id in list)
                {

                    FirebaseResponse response = client.Get("Course/" + id.id);
                    if (response.Body != "null")
                    {
                        dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
                        var json = string.Format("[{0}]", data);
                        Course[] editorial = JsonConvert.DeserializeObject<Course[]>(json);
                        if (editorial[0].Student != null)
                        {
                            if (editorial[0].Student.Contains(sid))
                            {

                                chat.Add(editorial[0].Coordinator, JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + editorial[0].Coordinator + "/Email").Body));
                                break;
                            }
                        }

                    }

                }



            }




            return View(chat);
        }
        public ActionResult Chat(string id)
        {
            if(id == null)
            {
                return RedirectToAction("index");
            }
            var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            var roleNow = prinicpal.Claims.Where(c => c.Type == "Role").Select(c => c.Value).SingleOrDefault();
            client = new FireSharp.FirebaseClient(config);

            ViewData["id"] = id;

            

            var messToBe = JsonConvert.DeserializeObject<Dictionary<string, string>>(client.Get("Chat/" + sid + "/" + id).Body);

            var messTo = new Dictionary<string, string>();
            if(messToBe != null)
            {
                foreach (var item in messToBe)
                {
                    var a = item.Key.Split(new Char[] { ' ' })[1];
                    messTo.Add(a, item.Value);
                }
            }
            var messFormBe = JsonConvert.DeserializeObject<Dictionary<string, string>>(client.Get("Chat/" + id + "/" + sid).Body);
            var messForm = new Dictionary<string, string>();
            if (messFormBe != null)
            {
                foreach (var item in messFormBe)
                {
                    var a = item.Key.Split(new Char[] { ' ' })[1];
                    messForm.Add(a, item.Value);
                }
            }


             

            var con = new Dictionary<string, List<string>>();
            int temp1 = 0;
            int temp2 = 0;
            if(messForm != null && messTo == null)
            {
                
                for (int i = 0; i < messForm.Count(); i++)
                {
                    var a = new List<string>();

                    
                        a.Add("I");
                        a.Add(messForm[i.ToString()]);
                        con.Add(i.ToString(), a);
                    

                }
            }
            else if (messForm == null && messTo != null)
            {
                for (int i = 0; i < messTo.Count(); i++)
                {
                    var a = new List<string>();


                    a.Add("U");
                    a.Add(messTo[i.ToString()]);
                    con.Add(i.ToString(), a);


                }
            }
            else if(messTo != null && messForm != null)
            {
                for (int i = 0; i < messTo.Count() + messForm.Count(); i++)
                {
                    var a = new List<string>();

                    if (messForm.ContainsKey(i.ToString()))
                    {
                        a.Add("I");
                        a.Add(messForm[i.ToString()]);
                        con.Add(i.ToString(), a);
                    }
                    else
                    {
                        a.Add("U");
                        a.Add(messTo[i.ToString()]);
                        con.Add(i.ToString(), a);
                    }


                }
            }

            if (roleNow == "Student")
            {
                ViewData["mail"] = JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + id + "/Email").Body);
            }
            else if (roleNow == "Marketing Coordinator")
            {
                
                ViewData["mail"] = JsonConvert.DeserializeObject<string>(client.Get("Account/Student/" + id + "/Email").Body);
            }
            return View(con);
        }
        [HttpPost]
        public ActionResult Chat(string id, string mess, string con)
        {
            if(con == null)
            {
                con = "0";
            }
            
            var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            var roleNow = prinicpal.Claims.Where(c => c.Type == "Role").Select(c => c.Value).SingleOrDefault();
            
            client = new FireSharp.FirebaseClient(config);
            var sent = new Dictionary<string, string>();
            sent.Add(con, mess);
            client.Set("Chat/" + sid + "/" + id+"/ "+con , mess);

            return RedirectToAction("chat", new {id = id });
        }
        }
}