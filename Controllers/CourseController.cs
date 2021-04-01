using Firebase.Storage;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Marketing Coordinator")]
    public class CourseController : Controller
    {
        // GET: Course



        // GET: Course

        private static IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "8Qcxfs4Nx3SwBX9iLWXKtDRyQ2DHZCBATJD075aF",
            BasePath = "https://aspdata-8d746-default-rtdb.europe-west1.firebasedatabase.app/"
        };
        private static IFirebaseClient client;
        private static string ApiKey = "AIzaSyCxf2rABg_dosQjVmNMh5-XJodMOU0_G04";
        private static string Bucket = "aspdata-8d746.appspot.com";


        [Authorize(Roles = "Marketing Coordinator")]
        public ActionResult Index()
        {
            var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();


            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse count = client.Get("Course/" + sid);

            var courses = new List<Course>();

            FirebaseResponse response = client.Get("Course/" + sid);
            if (response.Body != null && response.Body != "null")
            {
                dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
                var json = string.Format("[{0}]", data);
                Course[] course = JsonConvert.DeserializeObject<Course[]>(json);
                courses.Add(course[0]);



            }
            return View(courses);


        }
        [Authorize(Roles = "Marketing Coordinator")]
        // GET: Course/Details/5
        public ActionResult Details(int id)
        {
            var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Course/" + sid + "/" + id);
            var course = JsonConvert.DeserializeObject<Course>(response.Body);
            return View(course);
        }

        // GET: Course/Create


        // GET: Course/Edit/5
        public ActionResult Edit(string coordinator)
        {
            if (coordinator == null)
            {
                return RedirectToAction("Indexcoures");
            }

            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Course/" + coordinator);

            var courses = new Course();
            Course data = JsonConvert.DeserializeObject<Course>(response.Body);
            var json = string.Format("[{0}]", data);
            client = new FireSharp.FirebaseClient(config);
            string[] roleList = { "Student" };
            var list = new List<SignUpModel>();

            foreach (string role in roleList)
            {
                FirebaseResponse student = client.Get("Account/" + role);
                dynamic studentData = JsonConvert.DeserializeObject<dynamic>(student.Body);
                if (data != null)
                    foreach (var item in studentData)
                    {
                        list.Add(JsonConvert.DeserializeObject<SignUpModel>(((JProperty)item).Value.ToString()));
                    }
            }

            ViewData["students"] = list;

            return View(data);
        }

        // POST: Course/Edit/5
        [HttpPost]
        public ActionResult Edit(Course collection)
        {
            try
            {
                // TODO: Add update logic here
                var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();


                client = new FireSharp.FirebaseClient(config);
                if (collection.dateEnd == DateTime.Parse("1/1/0001 12:00:00 AM"))
                {
                    FirebaseResponse response = client.Get("Course/" + sid);
                    Course data = JsonConvert.DeserializeObject<Course>(response.Body);
                    collection.dateEnd = data.dateEnd;
                }
                collection.Coordinator = sid;
                SetResponse setResponse = client.Set("Course/" + sid, collection);
                var markResponse = client.Get("Mark/" + sid);
                var mark = JsonConvert.DeserializeObject<Dictionary<string, string>>(markResponse.Body);
                var markNew = new Dictionary<string, string>();
                foreach (var a in collection.Student)
                {
                    if (mark.ContainsKey(a))
                    {
                        markNew.Add(a, mark[a]);
                    }
                    else
                    {
                        markNew.Add(a, "Not Grade");
                    }
                }
                var add = client.SetAsync("Mark/" + sid, markNew);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Course/Delete/5
        public ActionResult Delete(int id)
        {

            return View();
        }

        // POST: Course/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
                var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
                client = new FireSharp.FirebaseClient(config);
                FirebaseResponse response = client.Delete("Course/" + sid + "/" + id);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        [Authorize(Roles = "Marketing Coordinator")]
        public ActionResult Mark()
        {
            try
            {

                var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
                var sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();


                client = new FireSharp.FirebaseClient(config);



                var response = client.Get("Mark/" + sid);
                var mark = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Body);

                var responseComment = client.Get("Comment/" + sid);
                var Comment = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseComment.Body);
                var b = new Dictionary<string, List<string>>();
                foreach (var a in mark)
                {
                    var c = new List<string>();
                    c.Add(JsonConvert.DeserializeObject<string>(client.Get("Account/Student/" + a.Key + "/Email").Body));
                    c.Add(a.Value);
                    if (Comment != null)
                    {
                        if (Comment.ContainsKey(a.Key))
                        {
                            c.Add(Comment[a.Key]);
                        }

                    }


                    b.Add(a.Key, c);
                }

                return View(b);
            }
            catch
            {
                return View();
            }
        }
        [Authorize(Roles = "Marketing Coordinator")]
        public async System.Threading.Tasks.Task<ActionResult> Marking(string id)
        {

            if (id == null)
            {
                return RedirectToAction("Index");
            }



            var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Link/" + sid + "/" + id);

            var token = prinicpal.Claims.Where(c => c.Type == "Token").Select(c => c.Value).SingleOrDefault();

            List<string> nameFile = new List<string>();
            List<string> link = new List<string>();
            if (response.Body != "null")
            {

                List<string> a = JsonConvert.DeserializeObject<List<string>>(response.Body);

                foreach (var item in a)
                {

                    var task = await new FirebaseStorage(Bucket, new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(token),
                        ThrowOnCancel = true
                    }).Child("Student submit").Child(sid).Child(id).Child(item).GetDownloadUrlAsync();
                    link.Add(task);
                    nameFile.Add(item);

                }

            }
            bool canMark = false;

            FirebaseResponse firebase = client.Get("Course/" + sid);

            var dateEnd = JsonConvert.DeserializeObject<Course>(firebase.Body).dateEnd;
            var dateNow = DateTime.Now;
            if (dateEnd > dateNow)
            {
                canMark = true;
            }
            else if (dateEnd.AddDays(14) > dateNow)
            {
                canMark = true;
            }

            var responseComment = client.Get("Comment/" + sid + "/" + id);
            var responseGrade = client.Get("Mark/" + sid + "/" + id);
            var Comment = JsonConvert.DeserializeObject<string>(responseComment.Body);
            ViewData["nameFile"] = nameFile;
            ViewData["link"] = link;
            ViewData["canMark"] = canMark;
            ViewData["Comment"] = Comment;
            ViewData["Mark"] = JsonConvert.DeserializeObject<string>(responseGrade.Body);

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Marking(string id, string grade, string comment)
        {
            var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            client = new FireSharp.FirebaseClient(config);
            var studentGrade = new Dictionary<string, string>();
            studentGrade.Add(id, grade);

            var studentComment = new Dictionary<string, string>();
            
            studentComment.Add(id, comment);

            if (grade == "Accept")
            {
                int exceptional = 0;
                if (client.Get("Exceptional/" + sid + "/" + id).Body != "null")
                {
                    exceptional = Int32.Parse(client.Get("Exceptional/" + sid + "/" + id).Body) + 1;
                }

                await client.SetAsync("Exceptional/" + sid + "/" + id, exceptional);
            }
            else
            {
                int exceptional = 1;
                await client.SetAsync("Exceptional/" + sid + "/" + id, exceptional);
            }

            await client.UpdateAsync("Mark/" + sid, studentGrade);
            await client.UpdateAsync("Comment/" + sid, studentComment);

            return RedirectToAction("Mark", new { });
        }
    }
}
