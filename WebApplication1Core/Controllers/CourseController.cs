﻿using Firebase.Storage;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using WebApplication1Core.Models;

namespace WebApplication1Core.Controllers
{
    [Authorize(Roles = "Marketing Coordinator")]
    public class CourseController : Controller
    {
        // GET: Course



        // GET: Course

        private static readonly IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "8Qcxfs4Nx3SwBX9iLWXKtDRyQ2DHZCBATJD075aF",
            BasePath = "https://aspdata-8d746-default-rtdb.europe-west1.firebasedatabase.app/"
        };
        private static IFirebaseClient? client;
        private static readonly string ApiKey = "AIzaSyCxf2rABg_dosQjVmNMh5-XJodMOU0_G04";
        private static readonly string Bucket = "aspdata-8d746.appspot.com";


        [Authorize(Roles = "Marketing Coordinator")]
        public ActionResult Index()
        {
            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();


            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse count = client.Get("Course/" + sid);

            List<Course> courses = new();

            FirebaseResponse response = client.Get("Course/" + sid);
            if (response.Body is not null and not "null")
            {
                dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
                dynamic json = string.Format("[{0}]", data);
                Course[] course = JsonConvert.DeserializeObject<Course[]>(json);
                courses.Add(course[0]);



            }
            return View(courses);


        }
        [Authorize(Roles = "Marketing Coordinator")]
        // GET: Course/Details/5
        public ActionResult Details(int id)
        {
            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Course/" + sid + "/" + id);
            Course course = JsonConvert.DeserializeObject<Course>(response.Body);
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
            _ = new Course();
            Course data = JsonConvert.DeserializeObject<Course>(response.Body);
            _ = string.Format("[{0}]", data);
            client = new FireSharp.FirebaseClient(config);
            string[] roleList = { "Student" };
            List<SignUpModel> list = new();

            foreach (string role in roleList)
            {
                FirebaseResponse student = client.Get("Account/" + role);
                dynamic studentData = JsonConvert.DeserializeObject<dynamic>(student.Body);
                if (data != null)
                {
                    foreach (object item in studentData)
                    {
                        list.Add(JsonConvert.DeserializeObject<SignUpModel>(((JProperty)item).Value.ToString()));
                    }
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
                ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();


                client = new FireSharp.FirebaseClient(config);
                if (collection.dateEnd == DateTime.Parse("1/1/0001 12:00:00 AM"))
                {
                    FirebaseResponse response = client.Get("Course/" + sid);
                    Course data = JsonConvert.DeserializeObject<Course>(response.Body);
                    collection.dateEnd = data.dateEnd;
                }
                collection.Coordinator = sid;
                SetResponse setResponse = client.Set("Course/" + sid, collection);
                FirebaseResponse markResponse = client.Get("Mark/" + sid);
                Dictionary<string, string> mark = JsonConvert.DeserializeObject<Dictionary<string, string>>(markResponse.Body);
                Dictionary<string, string> markNew = new();
                foreach (string a in collection.Student)
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
                Task<SetResponse> add = client.SetAsync("Mark/" + sid, markNew);
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
                ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
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

                ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
                string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();


                client = new FireSharp.FirebaseClient(config);



                FirebaseResponse response = client.Get("Mark/" + sid);
                Dictionary<string, string> mark = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Body);

                FirebaseResponse responseComment = client.Get("Comment/" + sid);
                Dictionary<string, string> Comment = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseComment.Body);
                Dictionary<string, List<string>> b = new();
                foreach (KeyValuePair<string, string> a in mark)
                {
                    List<string> c = new()
                    {
                        JsonConvert.DeserializeObject<string>(client.Get("Account/Student/" + a.Key + "/Email").Body),
                        a.Value
                    };
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
        public async Task<ActionResult> Marking(string id)
        {

            if (id == null)
            {
                return RedirectToAction("Index");
            }



            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Link/" + sid + "/" + id);

            string token = prinicpal.Claims.Where(c => c.Type == "Token").Select(c => c.Value).SingleOrDefault();

            List<string> nameFile = new();
            List<string> link = new();
            if (response.Body != "null")
            {

                List<string> a = JsonConvert.DeserializeObject<List<string>>(response.Body);

                foreach (string item in a)
                {

                    string task = await new FirebaseStorage(Bucket, new FirebaseStorageOptions
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

            DateTime dateEnd = JsonConvert.DeserializeObject<Course>(firebase.Body).dateEnd;
            DateTime dateNow = DateTime.Now;
            if (dateEnd > dateNow)
            {
                canMark = true;
            }
            else if (dateEnd.AddDays(14) > dateNow)
            {
                canMark = true;
            }

            FirebaseResponse responseComment = client.Get("Comment/" + sid + "/" + id);
            FirebaseResponse responseGrade = client.Get("Mark/" + sid + "/" + id);
            string Comment = JsonConvert.DeserializeObject<string>(responseComment.Body);
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
            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            client = new FireSharp.FirebaseClient(config);
            Dictionary<string, string> studentGrade = new()
            {
                { id, grade }
            };

            Dictionary<string, string> studentComment = new()
            {
                { id, comment }
            };

            if (grade == "Accept")
            {
                int exceptional = 0;
                if (client.Get("Exceptional/" + sid + "/" + id).Body != "null")
                {
                    exceptional = int.Parse(client.Get("Exceptional/" + sid + "/" + id).Body) + 1;
                }

                _ = await client.SetAsync("Exceptional/" + sid + "/" + id, exceptional);
            }
            else
            {
                int exceptional = 1;
                _ = await client.SetAsync("Exceptional/" + sid + "/" + id, exceptional);
            }

            _ = await client.UpdateAsync("Mark/" + sid, studentGrade);
            _ = await client.UpdateAsync("Comment/" + sid, studentComment);

            return RedirectToAction("Mark", new { });
        }
    }
}
