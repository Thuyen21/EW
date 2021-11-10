using Firebase.Auth;
using Firebase.Storage;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebApplication1.Models;
using FirebaseConfig = Firebase.Auth.FirebaseConfig;

namespace WebApplication1.Controllers
{


    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private static readonly IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "8Qcxfs4Nx3SwBX9iLWXKtDRyQ2DHZCBATJD075aF",
            BasePath = "https://aspdata-8d746-default-rtdb.europe-west1.firebasedatabase.app/"
        };
        private static IFirebaseClient client;
        private static readonly string ApiKey = "AIzaSyCxf2rABg_dosQjVmNMh5-XJodMOU0_G04";
        private static readonly string Bucket = "aspdata-8d746.appspot.com";



        // GET: Admin
        public ActionResult Index()
        {
            client = new FireSharp.FirebaseClient(config);
            string[] roleList = { "Admin", "Marketing Coordinator", "Marketing Manager", "Guest", "Student" };
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
            return View(list);
        }
        public ActionResult Edit()
        {
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Edit(string id, string role)
        {

            client = new FireSharp.FirebaseClient(config);
            if (id == null || role == null)
            {
                return RedirectToAction("Index");
            }



            FirebaseResponse response = client.Get("Account/" + role + "/" + id);
            dynamic data = JsonConvert.DeserializeObject<SignUpModel>(response.Body);


            return View(data);
        }
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> Edit(SignUpModel signUpModel, string oldRole, string oldEmail, string oldPassword)
        {
            try
            {
                if (signUpModel.Email != oldEmail || signUpModel.Password != oldPassword)
                {
                    FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                    FirebaseAuthLink token = await auth.SignInWithEmailAndPasswordAsync(oldEmail, oldPassword);
                    await auth.LinkAccountsAsync(token, signUpModel.Email, signUpModel.Password);
                }
                client = new FireSharp.FirebaseClient(config);
                if (signUpModel.Email == null)
                {
                    return RedirectToAction("Index");
                }
                FirebaseResponse response = client.Delete("Account/" + oldRole + "/" + signUpModel.id);
                SetResponse setResponse = client.Set("Account/" + signUpModel.role + "/" + signUpModel.id, signUpModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return RedirectToAction("Index");
        }

        public ActionResult Details()
        {
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Details(string id, string role)
        {
            client = new FireSharp.FirebaseClient(config);
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            FirebaseResponse response = client.Get("Account/" + role + "/" + id);
            dynamic data = JsonConvert.DeserializeObject<SignUpModel>(response.Body);



            return View(data);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult CreateCourse()
        {
            client = new FireSharp.FirebaseClient(config);

            List<string> students = new List<string>();
            List<string> mags = new List<string>();

            FirebaseResponse response = client.Get("Student/");

            Dictionary<string, string> dynamic = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Body);

            if (response.Body != "null")
            {
                foreach (KeyValuePair<string, string> a in dynamic)
                {
                    students.Add(a.Value);
                }
            }

            response = client.Get("Marketing Coordinator/");

            dynamic = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Body);

            if (response.Body != "null")
            {
                foreach (KeyValuePair<string, string> a in dynamic)
                {
                    mags.Add(a.Value);
                }
            }


            ViewData["students"] = students;
            ViewData["Marketing Coordinator"] = mags;
            return View();
        }

        // POST: Editorial/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateCourse(Course collection)
        {
            try
            {
                // TODO: Add insert logic here
                client = new FireSharp.FirebaseClient(config);


                SetResponse setResponse = client.Set("Editorial/" + collection.Coordinator, collection);



                Dictionary<string, string> studentGrade = new Dictionary<string, string>();
                if (collection.Student != null)
                {
                    foreach (string a in collection.Student)
                    {
                        studentGrade.Add(a, "Not Grade");
                    }
                }

                FirebaseResponse add = await client.UpdateAsync("Mark/" + collection.Coordinator, studentGrade);
                return RedirectToAction("IndexCourse");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return View();
            }
        }


        public ActionResult EditCourse(string coordinator)
        {
            if (coordinator == null)
            {
                return RedirectToAction("IndexCourse");
            }



            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Course/" + coordinator);

            Course courses = new Course();
            Course data = JsonConvert.DeserializeObject<Course>(response.Body);
            string json = string.Format("[{0}]", data);
            client = new FireSharp.FirebaseClient(config);


            return View(data);
        }

        // POST: Editorial/Edit/5
        [HttpPost]
        public async Task<ActionResult> EditCourse(Course collection)
        {
            try
            {
                client = new FireSharp.FirebaseClient(config);

                if (collection.dateEnd == DateTime.Parse("1/1/0001 12:00:00 AM"))
                {
                    FirebaseResponse response = await client.GetAsync("Course/" + collection.Coordinator);
                    Course data = JsonConvert.DeserializeObject<Course>(response.Body);
                    collection.dateEnd = data.dateEnd;

                }
                if (collection.dateFinal == DateTime.Parse("1/1/0001 12:00:00 AM"))
                {
                    FirebaseResponse response = await client.GetAsync("Course/" + collection.Coordinator);
                    Course data = JsonConvert.DeserializeObject<Course>(response.Body);
                    collection.dateFinal = data.dateFinal;
                }
                if (collection.dateFinal < collection.dateEnd)
                {
                    collection.dateFinal = collection.dateEnd;
                }

                await client.SetAsync("Course/" + collection.Coordinator, collection);


                FirebaseResponse markResponse1 = await client.GetAsync("Mark/" + collection.Coordinator);
                Dictionary<string, string> mark1 = JsonConvert.DeserializeObject<Dictionary<string, string>>(markResponse1.Body);
                Dictionary<string, string> markNew1 = new Dictionary<string, string>();
                if (markResponse1.Body != "null")
                {
                    if (collection.Student != null)
                    {
                        foreach (string a in collection.Student)
                        {
                            if (mark1.ContainsKey(a))
                            {
                                markNew1.Add(a, mark1[a]);
                            }
                            else
                            {
                                markNew1.Add(a, "Not Grade");
                            }
                        }
                        SetResponse add = await client.SetAsync("Mark/" + collection.Coordinator, markNew1);
                    }
                    else
                    {
                        client.Delete("Mark/" + collection.Coordinator);
                    }
                }
                else
                {
                    if (collection.Student != null)
                    {
                        foreach (string a in collection.Student)
                        {
                            markNew1.Add(a, "Not Grade");
                        }
                        SetResponse add = await client.SetAsync("Mark/" + collection.Coordinator, markNew1);
                    }
                    else
                    {
                        client.Delete("Mark/" + collection.Coordinator);
                    }

                }




                return RedirectToAction("IndexCourse");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult IndexCourse()
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
            List<Course> editorials = new List<Course>();
            Dictionary<string, string> mail = new Dictionary<string, string>();
            foreach (SignUpModel id in list)
            {
                FirebaseResponse response = client.Get("Course/" + id.id);
                if (response.Body != "null")
                {
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
                    dynamic json = string.Format("[{0}]", data);
                    Course[] editorial = JsonConvert.DeserializeObject<Course[]>(json);
                    editorials.Add(editorial[0]);
                    mail.Add(editorial[0].Coordinator, JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + editorial[0].Coordinator + "/Email").Body));
                }

            }
            FirebaseResponse guest = client.Get("Guest/");
            if (guest.Body != "null")
            {
                Dictionary<string, string> guests = JsonConvert.DeserializeObject<Dictionary<string, string>>(guest.Body);
                ViewData["guests"] = guests;
            }
            ViewData["mail"] = mail;
            return View(editorials);

        }
        //[HttpGet]
        //public ActionResult Create()
        //{
        //    return View();
        //}
        //[HttpPost]
        //public ActionResult Create(Admin admin)
        //{
        //    try
        //    {
        //        AddAdminToData(admin);
        //        ModelState.AddModelError(string.Empty, "Add Seccessfully");
        //    }

        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError(string.Empty, ex.Message);
        //    }
        //    return View();
        //}
        //private void AddAdminToData(Admin admin)
        //{
        //    client = new FireSharp.FirebaseClient(config);
        //    var data = admin;
        //    PushResponse response = client.Push("Admin/", data);
        //    data.adminEmail = response.Result.name;
        //    SetResponse setResponse = client.Set("Admin/" + data.adminEmail,data);

        //}
        public ActionResult CreateStudent()
        {
            client = new FireSharp.FirebaseClient(config);

            FirebaseResponse response = client.Get("Account/Marketing Coordinator/");

            List<SignUpModel> co = new List<SignUpModel>();
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            if (data != null)
            {
                foreach (dynamic item in data)
                {
                    co.Add(JsonConvert.DeserializeObject<SignUpModel>(((JProperty)item).Value.ToString()));
                }
            }

            ViewData["co"] = co;

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateStudent(SignUpModel model, string co)
        {
            try
            {
                model.role = "Student";
                FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));

                FirebaseAuthLink a = await auth.CreateUserWithEmailAndPasswordAsync(model.Email, model.Password, model.Name, true);



                client = new FireSharp.FirebaseClient(config);

                model.id = a.User.LocalId;
                SetResponse setResponse = client.Set("Account/" + model.role + "/" + model.id, model);

                string student = client.Get("Course/" + co + "/Student").Body;

                List<string> students = new List<string>();
                if (student != "null")
                {
                    List<string> add = JsonConvert.DeserializeObject<List<string>>(student);
                    foreach (string item in add)
                    {
                        students.Add(item);
                    }
                }


                students.Add(model.id);

                await client.SetAsync("Course/" + co + "/Student/", students);

                await client.SetAsync("Mark/" + co + "/" + model.id, "Not Grade");
                await client.SetAsync("Comment/" + co + "/" + model.id, " ");
                return RedirectToAction("Index");
            }

            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View();
        }
        public async Task<ActionResult> CreateAdmin()
        {

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateAdmin(SignUpModel model)
        {
            model.role = "Admin";
            FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));

            try
            {
                FirebaseAuthLink a = await auth.CreateUserWithEmailAndPasswordAsync(model.Email, model.Password, model.Name, true);
                client = new FireSharp.FirebaseClient(config);

                model.id = a.User.LocalId;
                SetResponse setResponse = client.Set("Account/" + model.role + "/" + model.id, model);


                ModelState.AddModelError(string.Empty, "Ok");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

            }
            return View();


        }

        public async Task<ActionResult> CreateMarketingCoordinator()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateMarketingCoordinator(SignUpModel model, string nameCourse)
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

            List<Course> courses = new List<Course>();
            foreach (SignUpModel id in list)
            {

                FirebaseResponse response = client.Get("Course/" + id.id);
                if (response.Body != "null")
                {
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
                    dynamic json = string.Format("[{0}]", data);
                    Course[] editorial = JsonConvert.DeserializeObject<Course[]>(json);
                    if (editorial[0].nameCourse == nameCourse)
                    {
                        ModelState.AddModelError(string.Empty, "Course name in using");
                        return View(model);
                    }

                }
            }

            try
            {
                model.role = "Marketing Coordinator";
                FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));

                FirebaseAuthLink a = await auth.CreateUserWithEmailAndPasswordAsync(model.Email, model.Password, model.Name, true);

                client = new FireSharp.FirebaseClient(config);

                model.id = a.User.LocalId;
                SetResponse setResponse = client.Set("Account/" + model.role + "/" + model.id, model);

                Course course = new Course
                {
                    Coordinator = model.id,
                    dateEnd = DateTime.Parse("1/1/0001 12:00:00 AM"),
                    dateFinal = DateTime.Parse("1/1/0001 12:00:00 AM"),
                    nameCourse = nameCourse
                };

                await client.SetAsync("Course/" + model.id, course);
                ModelState.AddModelError(string.Empty, "Ok");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

            }


            return View();
        }
        public async Task<ActionResult> CreateMarketingManager()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateMarketingManager(SignUpModel model)
        {
            try
            {
                model.role = "Marketing Manager";
                FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));

                FirebaseAuthLink a = await auth.CreateUserWithEmailAndPasswordAsync(model.Email, model.Password, model.Name, true);

                client = new FireSharp.FirebaseClient(config);

                model.id = a.User.LocalId;
                SetResponse setResponse = client.Set("Account/" + model.role + "/" + model.id, model);


                ModelState.AddModelError(string.Empty, "Ok");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

            }

            return View();
        }
        public async Task<ActionResult> CreateGuest()
        {
            client = new FireSharp.FirebaseClient(config);

            FirebaseResponse response = client.Get("Account/Marketing Coordinator/");

            List<SignUpModel> co = new List<SignUpModel>();
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);

            response = client.Get("Guest/");
            Dictionary<string, string> guested = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Body);
            if (data != null)
            {
                if (response.Body == "null")
                {
                    foreach (dynamic item in data)
                    {
                        co.Add(JsonConvert.DeserializeObject<SignUpModel>(((JProperty)item).Value.ToString()));


                    }
                }
                else
                {
                    foreach (dynamic item in data)
                    {
                        SignUpModel a = JsonConvert.DeserializeObject<SignUpModel>(((JProperty)item).Value.ToString());
                        if (guested.ContainsKey(a.id) == false)
                        {
                            co.Add(a);
                        }
                    }
                }
            }

            ViewData["co"] = co;


            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateGuest(SignUpModel model, string co)
        {
            try
            {
                model.role = "Guest";
                FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));

                FirebaseAuthLink a = await auth.CreateUserWithEmailAndPasswordAsync(model.Email, model.Password, model.Name, true);



                client = new FireSharp.FirebaseClient(config);

                model.id = a.User.LocalId;
                SetResponse setResponse = client.Set("Account/" + model.role + "/" + model.id, model);
                string guest = client.Get("Guest/").Body;
                if (guest != "null")
                {
                    Dictionary<string, string> b = JsonConvert.DeserializeObject<Dictionary<string, string>>(guest);
                    b.Add(co, model.id);

                    await client.SetAsync("Guest/", b);
                }
                else
                {
                    Dictionary<string, string> b = new Dictionary<string, string>
                    {
                        { co, model.id }
                    };

                    await client.SetAsync("Guest/", b);
                }



                //var guest = client.Get("Guest/" + co).Body;

                //List<string> students = new List<string>();
                //if (guest != "null")
                //{
                //    List<string> add = JsonConvert.DeserializeObject<List<string>>(guest);
                //    foreach (var item in add)
                //    {
                //        students.Add(item);
                //    }
                //}


                //students.Add(model.id);

                //await client.SetAsync("Guest/" + co, guest);



                return RedirectToAction("Index");
            }

            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View();
        }
        public ActionResult viewMark(string coordinator)
        {
            if (coordinator == null)
            {
                return RedirectToAction("IndexCourse");
            }
            client = new FireSharp.FirebaseClient(config);


            ViewData["coordinator"] = coordinator;

            FirebaseResponse response = client.Get("Mark/" + coordinator);
            Dictionary<string, string> mark = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Body);

            FirebaseResponse responseComment = client.Get("Comment/" + coordinator);
            Dictionary<string, string> Comment = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseComment.Body);

            Dictionary<string, List<string>> b = new Dictionary<string, List<string>>();
            Dictionary<string, string> mail = new Dictionary<string, string>();
            if (mark != null)
            {
                foreach (KeyValuePair<string, string> a in mark)
                {
                    mail.Add(a.Key, JsonConvert.DeserializeObject<string>(client.Get("Account/Student/" + a.Key + "/Email").Body));
                    List<string> c = new List<string>
                    {
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

            ViewData["mail"] = mail;


            return View(b);
        }
        public async System.Threading.Tasks.Task<ActionResult> viewSubmissions(string coordinator, string Student)
        {
            if (coordinator == null || Student == null)
            {
                return RedirectToAction("index");
            }
            client = new FireSharp.FirebaseClient(config);
            string ApiKey = "AIzaSyCxf2rABg_dosQjVmNMh5-XJodMOU0_G04";
            string Bucket = "aspdata-8d746.appspot.com";

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
            ViewData["coordinator"] = JsonConvert.DeserializeObject<string>(client.Get("Account/Marketing Coordinator/" + coordinator + "/Email").Body);

            ViewData["Student"] = JsonConvert.DeserializeObject<string>(client.Get("Account/Student/" + Student + "/Email").Body);
            return View();
        }
        public async System.Threading.Tasks.Task<ActionResult> Delete(string id, string role)
        {
            if (id == null || role == null)
            {
                return RedirectToAction("Index");
            }
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse firebase = await client.GetAsync("Account/" + role + "/" + id);
            dynamic data = JsonConvert.DeserializeObject<SignUpModel>(firebase.Body);

            return View(data);
        }
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> Delete(SignUpModel model)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));

            FirebaseAuthLink a = await auth.SignInWithEmailAndPasswordAsync(model.Email, model.Password);

            if (model.role == "Marketing Coordinator")
            {
                FirebaseResponse test1 = await client.GetAsync("Course/" + a.User.LocalId + "/Student");
                FirebaseResponse test2 = await client.GetAsync("Guest/" + a.User.LocalId);
                if (test1.Body != "null" || test2.Body != "null")
                {
                    ModelState.AddModelError(string.Empty, "Delete student and guest in this Marketing Coordinator first");
                    return View(model);

                }
                await client.DeleteAsync("Course/" + a.User.LocalId);
                await client.DeleteAsync("Comment/" + a.User.LocalId);
                await client.DeleteAsync("Link/" + a.User.LocalId);
                await client.DeleteAsync("Mark/" + a.User.LocalId);

            }

            else if (model.role == "Student")
            {
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
                            if (editorial[0].Student.Contains(model.id))
                            {
                                courses.Add(editorial[0]);
                                break;
                            }
                        }

                    }
                }
                string b1 = client.Get("Course/" + courses[0].Coordinator + "/Student").Body;
                if (b1 != "null")
                {
                    List<string> b = JsonConvert.DeserializeObject<List<string>>(b1);

                    b.Remove(a.User.LocalId);
                    await client.SetAsync("Course/" + courses[0].Coordinator + "/Student", b);
                }


                await client.DeleteAsync("Comment/" + courses[0].Coordinator + "/" + a.User.LocalId);
                await client.DeleteAsync("Mark/" + courses[0].Coordinator + "/" + a.User.LocalId);
                await client.DeleteAsync("Exceptional/" + courses[0].Coordinator + "/" + a.User.LocalId);

                string d = client.Get("Link/" + courses[0].Coordinator + "/student").Body;
                if (d != "null")
                {
                    List<string> c = JsonConvert.DeserializeObject<List<string>>(d);

                    c.Remove(a.User.LocalId);
                    await client.SetAsync("Link/" + courses[0].Coordinator + "/student", c);
                }

            }
            else if (model.role == "Guest")
            {
                string guest = client.Get("Guest/").Body;
                Dictionary<string, string> b = JsonConvert.DeserializeObject<Dictionary<string, string>>(guest);

                KeyValuePair<string, string> item = b.First(kvp => kvp.Value == a.User.LocalId);

                b.Remove(item.Key);
                if (b.Count > 0)
                {
                    await client.SetAsync("Guest/", b);
                }
                else
                {
                    await client.DeleteAsync("Guest/");
                }
            }

            await auth.DeleteUserAsync(a.FirebaseToken);
            await client.DeleteAsync("Account/" + model.role + "/" + model.id);
            return RedirectToAction("Index");
        }
    }
}