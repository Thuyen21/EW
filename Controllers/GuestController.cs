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
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Guest")]
    public class GuestController : Controller
    {
        private static IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "8Qcxfs4Nx3SwBX9iLWXKtDRyQ2DHZCBATJD075aF",
            BasePath = "https://aspdata-8d746-default-rtdb.europe-west1.firebasedatabase.app/"
        };
        private static IFirebaseClient client;
        private static string ApiKey = "AIzaSyCxf2rABg_dosQjVmNMh5-XJodMOU0_G04";
        private static string Bucket = "aspdata-8d746.appspot.com";
        // GET: Guest

        public ActionResult Index()
        {

            client = new FireSharp.FirebaseClient(config);
            var guest = client.Get("Guest/").Body;
            var b = JsonConvert.DeserializeObject<Dictionary<string, string>>(guest);
            var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();


            var item = b.First(kvp => kvp.Value == sid);
            ViewData["coordinator"] = item.Key;
            var response = client.Get("Mark/" + item.Key);
            var mark = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Body);

            var responseComment = client.Get("Comment/" + item.Key);
            var Comment = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseComment.Body);

            var d = new Dictionary<string, List<string>>();
            if(mark != null)
            {
                foreach (var a in mark)
                {
                    if (a.Value == "Accept")
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
                        d.Add(a.Key, c);
                    }

                }
            }
            

            return View(d);


            //string fileName = "~\\Content\\images";
            //string path = "https://firebasestorage.googleapis.com/v0/b/aspdata-8d746.appspot.com/o/Test%20(1)%20(2).docx?alt=media&token=48ffb5b7-142f-42e6-92dd-6d4c53235920";
            ////var webRootPath = Server.MapPath("~");
            //////var documentationPath = Path.GetFullPath(Path.Combine(webRootPath, path));
            //////var filePath = Path.GetFullPath(Path.Combine(documentationPath, fileName));

            ////var filePath = Path.GetFullPath(Path.Combine(path, fileName));
            ////ViewBag["test"] =  File(filePath, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            //WebClient myWebClient = new WebClient();
            ////myWebClient.DownloadFile(path, fileName);
            ////myWebClient.DownloadFile(path, @"d:\DZHosts\LocalUser\crt112233\www.crt112233.somee.com\Content\images\a.docx");

            ////string docText = System.IO.File.ReadAllText(@"D:\4 over\WebApplication3\WebApplication1\WebApplication1\Content\images\a.docx");


            //string[] filePaths = Directory.GetFiles(Server.MapPath("~\\Content\\images\\"+ sid));


            //List<FileModel> files = new List<FileModel>();
            //foreach (string filePath in filePaths)
            //{
            //    files.Add(new FileModel()
            //    {
            //        FileName = Path.GetFileName(filePath),
            //        FilePath = filePath
            //    });
            //}
            //using (ZipFile zip = new ZipFile())
            //{
            //    zip.AlternateEncodingUsage = ZipOption.AsNecessary;
            //    zip.AddDirectoryByName("Files");
            //    foreach (FileModel file in files)
            //    {

            //        zip.AddFile(file.FilePath, "Files");

            //    }
            //    string zipName = String.Format("Zip_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
            //    using (MemoryStream memoryStream = new MemoryStream())
            //    {
            //        zip.Save(memoryStream);
            //        return File(memoryStream.ToArray(), "application/zip", zipName);
            //    }
            //}

            //string FileName = "c.docx";
            //object documentFormat = 8;
            //string randomName = DateTime.Now.Ticks.ToString();
            //object htmlFilePath = Server.MapPath("~/Content/images/") + randomName + ".htm";
            //object fileSavePath = Server.MapPath("~/Content/images/") + Path.GetFileName(FileName);
            //_Application applicationclass = new Application();
            //applicationclass.Documents.Open(ref fileSavePath);
            //applicationclass.Visible = false;
            //Document document = applicationclass.ActiveDocument;
            //document.SaveAs(ref htmlFilePath, ref documentFormat);
            //document.Close();
            //string wordHTML = System.IO.File.ReadAllText(htmlFilePath.ToString());
            //return Content(wordHTML);

            //return RedirectToAction("Read", "PreviewWord", new { path = "https://firebasestorage.googleapis.com/v0/b/aspdata-8d746.appspot.com/o/Student%20submit%2F2yIqUEMXTEPptZMDjZinVL9r7Sv1%2F8%2FfAggg00LiFT7FKr56WvIMmQg7YI3%2Fc.docx?alt=media&token=5c90773c-c0d2-4868-a415-daa245d0f8c7", target = "_blank" });

        }

        // GET: Guest/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Guest/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Guest/Create
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

        // GET: Guest/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Guest/Edit/5
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

        // GET: Guest/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Guest/Delete/5
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
        public async System.Threading.Tasks.Task<ActionResult> viewSubmissions(string coordinator, string Student)
        {
            if (coordinator == null || Student == null)
            {
                return RedirectToAction("index");
            }
            client = new FireSharp.FirebaseClient(config);


            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Link/" + coordinator + "/" + Student);
            var prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
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
    }
}
