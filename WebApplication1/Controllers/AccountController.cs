using Firebase.Auth;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;




namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {

        private static readonly IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "8Qcxfs4Nx3SwBX9iLWXKtDRyQ2DHZCBATJD075aF",
            BasePath = "https://aspdata-8d746-default-rtdb.europe-west1.firebasedatabase.app/"
        };

        private static IFirebaseClient client;


        private static readonly string ApiKey = "AIzaSyCxf2rABg_dosQjVmNMh5-XJodMOU0_G04";
        private static readonly string Bucket = "aspdata-8d746.appspot.com";
        // GET: Account

        [Authorize(Roles = "Admin")]
        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SignUp(SignUpModel model)
        {
            try
            {
                FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));

                FirebaseAuthLink a = await auth.CreateUserWithEmailAndPasswordAsync(model.Email, model.Password, model.Name, true);

                client = new FireSharp.FirebaseClient(config);

                model.id = a.User.LocalId;
                SetResponse setResponse = client.Set("Account/" + model.role + "/" + model.id, model);



                ModelState.AddModelError(string.Empty, "Please Verify your email then login Plz.");
            }

            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> Login(string returnUrl)
        {
            try
            {
                //var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                //var ab = await auth.CreateUserWithEmailAndPasswordAsync("admin@gmail.com", "adminadmin");


                //SignUpModel model = new SignUpModel();
                //client = new FireSharp.FirebaseClient(config);



                //model.id = ab.User.LocalId;
                //model.Email = "admin@gmail.com";
                //model.Password = "adminadmin";
                //model.role = "Admin";
                //model.enable = true;
                //model.Name = "Admin";
                //SetResponse setResponse = client.Set("Account/" + model.role + "/" + model.id, model);

                if (Request.IsAuthenticated)
                {



                    //  return this.RedirectToLocal(returnUrl);
                }
            }
            catch (Exception ex)
            {

                Console.Write(ex);
            }


            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {

            try
            {

                if (ModelState.IsValid)
                {
                    FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                    FirebaseAuthLink ab = await auth.SignInWithEmailAndPasswordAsync(model.Email, model.Password);
                    string token = ab.FirebaseToken;

                    User user = ab.User;

                    //if(ab.User.IsEmailVerified == false)
                    //{
                    //    ModelState.AddModelError(string.Empty, "Please Verify your email then login Plz.");
                    //    return View(model);
                    //}
                    if (token == null)
                    {
                        ModelState.AddModelError("error", "Invalid username or password.");
                        return View(model);
                    }
                    List<Claim> claims = new List<Claim>();
                    client = new FireSharp.FirebaseClient(config);



                    string[] role = { "Admin", "Guest", "Marketing Coordinator", "Marketing Manager", "Student" };
                    FirebaseResponse enable = client.Get("");
                    foreach (string test in role)
                    {
                        enable = client.Get("Account/" + test + "/" + user.LocalId);
                        if (enable.Body != "null")
                        {
                            model.role = test;
                            break;
                        }
                    }


                    SignUpModel data = JsonConvert.DeserializeObject<SignUpModel>(enable.Body);
                    if (data == null)
                    {
                        ModelState.AddModelError("error", "Input right role");
                        return View(model);
                    }
                    if (data.enable == false)
                    {
                        ModelState.AddModelError("error", "Your acc had disable");
                        return View(model);
                    }


                    try
                    {
                        claims.Add(new Claim(ClaimTypes.Email, model.Email));
                        claims.Add(new Claim(ClaimTypes.Sid, ab.User.LocalId));
                        claims.Add(new Claim(ClaimTypes.Authentication, token));
                        claims.Add(new Claim("Token", token));
                        claims.Add(new Claim("Name", data.Name));
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, token));
                        claims.Add(new Claim(ClaimTypes.Role, data.role));
                        claims.Add(new Claim("Role", data.role));


                        ClaimsIdentity claimIdenties = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

                        Microsoft.Owin.IOwinContext ctx = Request.GetOwinContext();
                        IAuthenticationManager authenticationManager = ctx.Authentication;


                        authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = true }, claimIdenties);

                        ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;

                        // Gets list of claims.


                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                    User a = await auth.GetUserAsync(token);

                    ModelState.AddModelError("error", a.LocalId);




                    return RedirectToAction("Index", "Home");
                    //return this.RedirectToLocal(returnUrl);


                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("error", "Invalid username or password.");
                Console.Write(ex);
            }


            return View(model);
        }

        private void SignInUser(string email, string token, bool isPersistent)
        {


        }

        private void ClaimIdentities(string username, bool isPersistent)
        {

            List<Claim> claims = new List<Claim>();
            try
            {

                claims.Add(new Claim(ClaimTypes.Name, username));
                ClaimsIdentity claimIdenties = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            try
            {

                if (Url.IsLocalUrl(returnUrl))
                {

                    return Redirect(returnUrl);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }


            return RedirectToAction("LogOff", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult LogOff()
        {
            Microsoft.Owin.IOwinContext ctx = Request.GetOwinContext();
            IAuthenticationManager authenticationManager = ctx.Authentication;
            authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");

        }


    }
}