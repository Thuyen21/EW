using Firebase.Storage;
using FireSharp.Interfaces;
using FireSharp.Response;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Marketing Manager")]
    public class MarketingManagerController : Controller
    {
        private static readonly IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "8Qcxfs4Nx3SwBX9iLWXKtDRyQ2DHZCBATJD075aF",
            BasePath = "https://aspdata-8d746-default-rtdb.europe-west1.firebasedatabase.app/"
        };
        private static IFirebaseClient client;
        private static readonly string ApiKey = "AIzaSyCxf2rABg_dosQjVmNMh5-XJodMOU0_G04";
        private static readonly string Bucket = "aspdata-8d746.appspot.com";


        // GET: MarketingManager
        public ActionResult Index()
        {
            client = new FireSharp.FirebaseClient(config);
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
            List<Course> Courses = new List<Course>();
            Dictionary<string, string> mail = new Dictionary<string, string>();
            foreach (SignUpModel id in list)
            {

                FirebaseResponse response = client.Get("Course/" + id.id);
                if (response.Body != null && response.Body != "null")
                {
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
                    dynamic json = string.Format("[{0}]", data);
                    Course[] Course = JsonConvert.DeserializeObject<Course[]>(json);
                    Courses.Add(Course[0]);
                    mail.Add(Course[0].Coordinator, JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + Course[0].Coordinator + "/Email").Body));
                }



            }
            ViewData["mail"] = mail;

            return View(Courses);

        }
        public ActionResult viewMark(string coordinator)
        {
            if (coordinator == null)
            {
                return RedirectToAction("index");
            }
            client = new FireSharp.FirebaseClient(config);


            ViewData["coordinator"] = coordinator;

            FirebaseResponse response = client.Get("Mark/" + coordinator);
            Dictionary<string, string> mark = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Body);
            Dictionary<string, string> markSort = new Dictionary<string, string>();
            if (mark != null)
            {
                foreach (KeyValuePair<string, string> item in mark)
                {
                    if (item.Value == "Accept")
                    {
                        markSort.Add(item.Key, item.Value);
                    }
                }
            }


            FirebaseResponse responseComment = client.Get("Comment/" + coordinator);
            Dictionary<string, string> Comment = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseComment.Body);

            Dictionary<string, List<string>> b = new Dictionary<string, List<string>>();
            if (markSort != null)
            {
                foreach (KeyValuePair<string, string> a in markSort)
                {
                    List<string> c = new List<string>
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
            }

            return View(b);
        }
        public async System.Threading.Tasks.Task<ActionResult> viewSubmissions(string coordinator, string Student)
        {
            if (coordinator == null || Student == null)
            {
                return RedirectToAction("index");
            }
            client = new FireSharp.FirebaseClient(config);

            FirebaseResponse response = client.Get("Link/" + coordinator + "/" + Student);
            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string token = prinicpal.Claims.Where(c => c.Type == "Token").Select(c => c.Value).SingleOrDefault();
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
                    }).Child("Student submit").Child(coordinator).Child(Student).Child(item).GetDownloadUrlAsync();
                    link.Add(task);
                    nameFile.Add(item);
                }

            }
            ViewData["nameFile"] = nameFile;
            ViewData["link"] = link;
            ViewData["coordinator"] = coordinator;

            ViewData["Student"] = Student;
            return View();
        }


        // GET: MarketingManager/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: MarketingManager/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MarketingManager/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: MarketingManager/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: MarketingManager/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: MarketingManager/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MarketingManager/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        public async Task<ActionResult> DownloadZip(string coordinator, string student)
        {
            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            if (coordinator == null || student == null)
            {
                return RedirectToAction("index");
            }
            client = new FireSharp.FirebaseClient(config);

            FirebaseResponse response = client.Get("Link/" + coordinator + "/" + student);

            string token = prinicpal.Claims.Where(c => c.Type == "Token").Select(c => c.Value).SingleOrDefault();

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
                    }).Child("Student submit").Child(coordinator).Child(student).Child(item).GetDownloadUrlAsync();
                    link.Add(task);
                    nameFile.Add(item);



                }

            }

            string fileName = Server.MapPath("~\\Content\\images\\" + sid);

            WebClient myWebClient = new WebClient();

            DirectoryInfo attachments_AR = new DirectoryInfo(fileName);
            EmptyFolder(attachments_AR);

            if (!Directory.Exists(fileName))
            {
                Directory.CreateDirectory(fileName);
            }


            for (int i = 0; i < link.Count; i++)
            {

                myWebClient.DownloadFile(link[i], @"d:\DZHosts\LocalUser\crt112233\www.crt112233.somee.com\Content\images\" + sid + @"\" + nameFile[i]);
            }


            string[] filePaths = Directory.GetFiles(Server.MapPath("~\\Content\\images\\" + sid));


            List<FileModel> files = new List<FileModel>();
            foreach (string filePath in filePaths)
            {
                files.Add(new FileModel()
                {
                    FileName = Path.GetFileName(filePath),
                    FilePath = filePath
                });
            }
            using (ZipFile zip = new ZipFile())
            {
                zip.AlternateEncodingUsage = ZipOption.AsNecessary;
                zip.AddDirectoryByName("Files");
                foreach (FileModel file in files)
                {

                    zip.AddFile(file.FilePath, "Files");

                }
                string zipName = string.Format("Zip_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    zip.Save(memoryStream);
                    return File(memoryStream.ToArray(), "application/zip", zipName);
                }
            }
        }
        private void EmptyFolder(DirectoryInfo directory)
        {

            try
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo subdirectory in directory.GetDirectories())
                {
                    EmptyFolder(subdirectory);
                    subdirectory.Delete();
                }
            }
            catch
            {

            }


        }
        public async Task<ActionResult> Exceptional(string coordinator)
        {
            if (coordinator == null)
            {
                return RedirectToAction("index");
            }
            client = new FireSharp.FirebaseClient(config);


            Dictionary<string, string> exceptional = JsonConvert.DeserializeObject<Dictionary<string, string>>(client.Get("Exceptional/" + coordinator).Body);
            Dictionary<string, List<string>> b = new Dictionary<string, List<string>>();
            Dictionary<string, string> matches = new Dictionary<string, string>();
            if (exceptional != null)
            {
                List<string> matche = exceptional.Where(pair => pair.Value == "0").Select(pair => pair.Key).ToList();
                if (matche.Count() > 0)
                {
                    foreach (string item in matche)
                    {
                        string test = JsonConvert.DeserializeObject<string>(client.Get("Comment/" + coordinator + "/" + item).Body);
                        if (test == " " || test == "")
                        {

                            matches.Add(item, JsonConvert.DeserializeObject<string>(client.Get("Account/Student/" + item + "/Email").Body));
                        }

                    }
                }

                //var response = client.Get("Mark/" + coordinator);
                //var mark = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Body);

                //var responseComment = client.Get("Comment/" + coordinator);
                //var Comment = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseComment.Body);


                //foreach (var a in mark)
                //{
                //    if (matches.Contains(a.Key))
                //    {
                //        var c = new List<string>();
                //        c.Add(a.Value);
                //        if (Comment != null)
                //        {
                //            if (Comment.ContainsKey(a.Key))
                //            {
                //                c.Add(Comment[a.Key]);
                //            }

                //        }


                //        b.Add(a.Key, c);
                //    }

                //}
            }

            ViewData["coordinator"] = coordinator;



            return View(matches);
        }
        public async Task<ActionResult> Contributions(string coordinator)
        {
            if (coordinator == null)
            {
                return RedirectToAction("index");
            }
            client = new FireSharp.FirebaseClient(config);



            FirebaseResponse student = client.Get("Link/" + coordinator + "/student");
            List<string> students = JsonConvert.DeserializeObject<List<string>>(student.Body);

            Dictionary<string, int> b = new Dictionary<string, int>();
            if (students != null)
            {
                foreach (string item in students)
                {
                    List<string> contribution = JsonConvert.DeserializeObject<List<string>>(client.Get("Link/" + coordinator + "/" + item).Body);
                    if (contribution == null)
                    {
                        b.Add(item, 0);
                    }
                    else
                    {
                        b.Add(item, contribution.Count());
                    }

                }
            }


            ViewData["b"] = b;



            List<SignUpModel> list = new List<SignUpModel>();
            Dictionary<string, int> c = new Dictionary<string, int>();

            Dictionary<string, int> d = new Dictionary<string, int>();
            FirebaseResponse response = client.Get("Account/Marketing Coordinator");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            if (data != null)
            {
                foreach (dynamic item in data)
                {
                    list.Add(JsonConvert.DeserializeObject<SignUpModel>(((JProperty)item).Value.ToString()));
                }
            }

            foreach (SignUpModel item in list)
            {
                List<string> stu = JsonConvert.DeserializeObject<List<string>>(client.Get("Link/" + item.id + "/student").Body);
                if (stu != null)
                {
                    int e = 0;
                    foreach (string item1 in stu)
                    {
                        List<string> sub = JsonConvert.DeserializeObject<List<string>>(client.Get("Link/" + item.id + "/" + item1).Body);
                        if (sub == null)
                        {
                            c.Add(item1, 0);

                        }
                        else
                        {
                            c.Add(item1, sub.Count());
                            e = e + sub.Count();
                        }


                    }

                    d.Add(item.id, e);

                }


            }

            ViewData["d"] = d;
            ViewData["c"] = c;
            return View();
        }

    }
}
