using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ChatController : Controller

    {
        private static readonly IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "8Qcxfs4Nx3SwBX9iLWXKtDRyQ2DHZCBATJD075aF",
            BasePath = "https://aspdata-8d746-default-rtdb.europe-west1.firebasedatabase.app/"
        };
        private static IFirebaseClient client;
        private static readonly string ApiKey = "AIzaSyCxf2rABg_dosQjVmNMh5-XJodMOU0_G04";
        private static readonly string Bucket = "aspdata-8d746.appspot.com";
        // GET: Chat
        public ActionResult Index()
        {
            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            string roleNow = prinicpal.Claims.Where(c => c.Type == "Role").Select(c => c.Value).SingleOrDefault();
            client = new FireSharp.FirebaseClient(config);
            Dictionary<string, string> chat = new Dictionary<string, string>();
            string[] roleList = new string[1];
            if (roleNow == "Marketing Coordinator")
            {
                FirebaseResponse course = client.Get("Course/" + sid);
                if (course.Body != "null")
                {
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(course.Body);
                    dynamic json = string.Format("[{0}]", data);
                    Course[] editorial = JsonConvert.DeserializeObject<Course[]>(json);
                    if (editorial[0].Student != null)
                    {
                        foreach (string item in editorial[0].Student)
                        {
                            chat.Add(item, JsonConvert.DeserializeObject<string>(client.Get("Account/Student/" + item + "/Email").Body));
                        }
                    }


                }


            }
            else if (roleNow == "Student")
            {
                roleList = new string[] { "Marketing Coordinator" };
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
            if (id == null)
            {
                return RedirectToAction("index");
            }
            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            string roleNow = prinicpal.Claims.Where(c => c.Type == "Role").Select(c => c.Value).SingleOrDefault();
            client = new FireSharp.FirebaseClient(config);

            ViewData["id"] = id;



            Dictionary<string, string> messToBe = JsonConvert.DeserializeObject<Dictionary<string, string>>(client.Get("Chat/" + sid + "/" + id).Body);

            Dictionary<string, string> messTo = new Dictionary<string, string>();
            if (messToBe != null)
            {
                foreach (KeyValuePair<string, string> item in messToBe)
                {
                    string a = item.Key.Split(new char[] { ' ' })[1];
                    messTo.Add(a, item.Value);
                }
            }
            Dictionary<string, string> messFormBe = JsonConvert.DeserializeObject<Dictionary<string, string>>(client.Get("Chat/" + id + "/" + sid).Body);
            Dictionary<string, string> messForm = new Dictionary<string, string>();
            if (messFormBe != null)
            {
                foreach (KeyValuePair<string, string> item in messFormBe)
                {
                    string a = item.Key.Split(new char[] { ' ' })[1];
                    messForm.Add(a, item.Value);
                }
            }




            Dictionary<string, List<string>> con = new Dictionary<string, List<string>>();
            int temp1 = 0;
            int temp2 = 0;
            if (messForm != null && messTo == null)
            {

                for (int i = 0; i < messForm.Count(); i++)
                {
                    List<string> a = new List<string>
                    {
                        "I",
                        messForm[i.ToString()]
                    };
                    con.Add(i.ToString(), a);


                }
            }
            else if (messForm == null && messTo != null)
            {
                for (int i = 0; i < messTo.Count(); i++)
                {
                    List<string> a = new List<string>
                    {
                        "U",
                        messTo[i.ToString()]
                    };
                    con.Add(i.ToString(), a);


                }
            }
            else if (messTo != null && messForm != null)
            {
                for (int i = 0; i < messTo.Count() + messForm.Count(); i++)
                {
                    List<string> a = new List<string>();

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
            if (con == null)
            {
                con = "0";
            }

            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            string roleNow = prinicpal.Claims.Where(c => c.Type == "Role").Select(c => c.Value).SingleOrDefault();

            client = new FireSharp.FirebaseClient(config);
            Dictionary<string, string> sent = new Dictionary<string, string>
            {
                { con, mess }
            };
            client.Set("Chat/" + sid + "/" + id + "/ " + con, mess);

            return RedirectToAction("chat", new { id = id });
        }
    }
}