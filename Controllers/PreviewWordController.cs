using Microsoft.Office.Interop.Word;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class PreviewWordController : Controller
    {
        // GET: PreviewWord
        public ActionResult Read(string path)
        {
            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            string fileName = Request.MapPath("~/Content/images/" + sid + ".docx");

            if (System.IO.File.Exists(fileName))
            {
                System.IO.File.Delete(fileName);
            }

            WebClient myWebClient = new WebClient();




            myWebClient.DownloadFile(path, @"D:\4 over\WebApplication3\WebApplication1\WebApplication1\Content\images\" + sid + ".docx");
            ////myWebClient.DownloadFile(path, @"d:\DZHosts\LocalUser\crt112233\www.crt112233.somee.com\Content\images\a.docx");

            string FileName = sid + ".docx";
            object documentFormat = 8;
            string randomName = DateTime.Now.Ticks.ToString();
            object htmlFilePath = Server.MapPath("~/Content/images/") + randomName + ".htm";
            object fileSavePath = Server.MapPath("~/Content/images/") + Path.GetFileName(FileName);
            _Application applicationclass = new Application();
            applicationclass.Documents.Open(ref fileSavePath);
            applicationclass.Visible = false;
            Document document = applicationclass.ActiveDocument;
            document.SaveAs(ref htmlFilePath, ref documentFormat);
            document.Close();
            string wordHTML = System.IO.File.ReadAllText(htmlFilePath.ToString());

            return Content(wordHTML);

            //return View();

        }

        public ActionResult WordToPdf(string path, string name)
        {
            ClaimsPrincipal prinicpal = (ClaimsPrincipal)Thread.CurrentPrincipal;
            string sid = prinicpal.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();

            string fileName = Request.MapPath("~/Content/images/" + sid);


            DirectoryInfo attachments_AR = new DirectoryInfo(fileName);
            EmptyFolder(attachments_AR);


            if (!Directory.Exists(fileName))
            {
                Directory.CreateDirectory(fileName);
            }

            WebClient myWebClient = new WebClient();
            myWebClient.DownloadFile(path, @"d:\DZHosts\LocalUser\crt112233\www.crt112233.somee.com\Content\images\" + sid + @"\" + name);


            string fullpath = Server.MapPath("~/Content/images/" + sid + "/" + name);
            byte[] FileBytes = System.IO.File.ReadAllBytes(ConvertWordToPdf(fullpath, name, sid));

            int contentLength = FileBytes.Length;
            Response.AppendHeader("Content-Length", contentLength.ToString());
            Response.AppendHeader("Content-Disposition", "inline; filename=" + name + ".pdf");

            return File(FileBytes, "application/pdf;");

        }
        public string ConvertWordToPdf(string fullpath, string name, string sid)
        {
            WordDocument wordDocument = new WordDocument(fullpath, FormatType.Docx);

            DocToPDFConverter converter = new DocToPDFConverter();
            PdfDocument pdfDocument = converter.ConvertToPDF(wordDocument);
            converter.Dispose();
            wordDocument.Close();

            string path = Path.Combine(Server.MapPath("~/Content/images/" + sid), name + ".pdf");
            pdfDocument.Save(path);
            pdfDocument.Close(true);
            return path;
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
    }
}