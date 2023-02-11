/*
=======================================================
* Program Name: Bingo Vintage - v1.0
* Author: Crivelli José Marcos
* License: https://github.com/JMC2022/JMC2022.github.io/blob/main/LICENSE
======================================================== */
using BingoVintage.Models;
using BingoVintage.Rules;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BingoVintage.ExtentionMethods;

namespace BingoVintage.Controllers
{
    public class HomeController : Controller
    {
        //Get current cookie from browser
        private ClaimsPrincipal Claim { get {return HttpContext.User; } }
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult Credits()
        {
            return View();
        }

        // Login.
        public IActionResult Login()
        {
            if (User.HasClaim(u=> u.Value != null))
            {
                return RedirectToAction("Saloon");
            }
            return View();
        }

        [HttpPost]
        public async Task <IActionResult> Login(Usr u)
        {
            try
            {
                if (!ModelState.IsValid || User.HasClaim (u=> u.Value == null))
                {
                    return View("Login", u);
                }
                //Search Usr in DB.
                Response rule = new UsrRule().Login(u);
                if (!rule.Result)
                {
                    ViewData["Message"] = rule.Message!.ToString();
                    //Usr not Registered - Invalid Access.
                    if (rule.Obj == null)
                    {
                        return View("Login", u);
                    }
                    //Invalid Email - Check Token - Invalid Token.
                    return View("CreateUsr", u);
                }
                else if(rule.Code == "22") 
                {
                    ViewData["Message"] = rule.Message!.ToString(); //User was succesfully createrd - Log In
                    return View("Login");
                }

                // Convert Response object to Usr.
                Usr player = (Usr)rule.Obj!;

                //Start a claim to insert cookie on browser for validation Usr.
                var claims = new List<Claim>() {
                        new Claim(ClaimTypes.NameIdentifier, Convert.ToString(player.UsrID)),
                        new Claim(ClaimTypes.Name, player.Name),
                        new Claim(ClaimTypes.Email, player.Email),
                        new Claim(ClaimTypes.Role, "Player"),
                        };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties()
                {
                    IsPersistent = u.RememberLogin
                });

                return RedirectToAction("Saloon", u);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
        
        // Register Usr.
        public IActionResult Register()
        {
            if (User.HasClaim(u => u.Value != null))
            {
                return RedirectToAction("Saloon");
            }
            return View();
        }

        // Create Usr.
        [HttpPost]
        public IActionResult CreateUsr(Usr u)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Register", u);
                }
                Response rule = new UsrRule().CreateUsr(u);
                if (!rule.Result)
                {
                    ViewData["Message"] = rule.Message!.ToString();
                    return View("Register", u);
                }
                ViewData["Message"] = rule.Message!.ToString();
                return View("CreateUsr");
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        // Saloon.
        [Authorize]
        public IActionResult Saloon()
        {
            //Sanity check
            var usrId = new UsrMethod().CompareIdUsrCookieToDB(Claim);
            var rule = new SaloonRule().GetListOfTickets(usrId);
            List<Ticket> tickets = rule;
            if (usrId == 0 || rule.Count > 4) /*|| (User.IsInRole("Player") && rule.Count > 0))*/
            {
                return RedirectToAction("Logout");
            }
            else if (User.IsInRole("Playing"))
            {
                // Avoid wrong Usr to start with Cookie in cache or Start Playing with more than 4 Tickets.
                if (rule.Count == 0)
                    return RedirectToAction("Logout");

                return View(tickets);
            }
            // If User leave the page without finish a game , tickets can be active and we need to restart it.
            else if (User.IsInRole("Player") && rule.Count > 0)
            {
                new SaloonRule().DisableT(Claim);
            }
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult Saloon(Usr u)
        {
            try
            {
                if (User.HasClaim(u => u.Value != null) && ModelState.IsValid)
                    return View(u);
                
                return View("Login");
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [Authorize]
        // Call all needed to Get Tickets and save to temporary DB
        public PartialViewResult Ticket(int cantT) 
        {
            //If cookie get hacked just show a view error... lool
            if (Claim != null)
            {
                try
                {
                    var compareCookie = new UsrMethod().CompareIdUsrCookieToDB(Claim);
                    if (compareCookie == 0)
                    {
                        return PartialView("Error");
                    }
                    IEnumerable<Ticket> rule = new SaloonRule().CreateTempTicket(cantT, Claim);
                    return PartialView("_Tikets", rule);
                }
                catch (Exception)
                {
                    return PartialView("Error");
                }
            }
            else
            {
                return PartialView("Error");
            }
        }

        [Authorize]
        // Call all needed to Start Playing
        public async Task<PartialViewResult> Start() 
        {
            // Sanity check
            var usrId = new UsrMethod().CompareIdUsrCookieToDB(Claim);
            if (usrId != 0)
            {
                try
                {
                    // Create Playing Tickets on DB and ad History
                    new SaloonRule().SaveTktIntoDB();

                    // Change the User Role
                    ClaimsIdentity? i = Claim.Identities.FirstOrDefault();
                    var actualRole = i!.Claims.ElementAt(3);
                    i.RemoveClaim(actualRole);
                    var newRole = new Claim(ClaimTypes.Role, "Playing");
                    i!.AddClaim(newRole);

                    //Get how many Tickets are playing and insert into "RSA" identity.
                    var cantT = new SaloonRule().GetCantT(Claim);
                    var Rsa = new Claim(ClaimTypes.Rsa, cantT.ToString());
                    i!.AddClaim(Rsa);

                    //Add UserData Identity for know the winning game , at first is empty.
                    var winning = new Claim(ClaimTypes.UserData, "empty");
                    i!.AddClaim(winning);

                    var principal = new ClaimsPrincipal(i);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties());

                    List<Ticket> tickets = new SaloonRule().GetListOfTickets(usrId);
                    return PartialView("_Tikets", tickets);
                }
                catch (Exception)
                {
                    return PartialView("Error");
                }
            }
            else
            {
                return PartialView("Error");
            }
        }

        //Launch is enable till "Bingo" apears. It return the acert number if ticket has one or more.
        [Authorize]
        public PartialViewResult LaunchBall()
        {
            var rule = new SaloonRule().LaunchBall(Claim);
            return PartialView("_BallNo", rule);
        }

        [Authorize]
        // Return an Array with TicketNo, NumId, Acert, and Num
        public Array? CompareAcert()
        {
            var usrId = new UsrMethod().CompareIdUsrCookieToDB(Claim);
            var dataAcert = new SaloonRule().PrintAcert(usrId)!;
            if (dataAcert !=null)
            {
                return dataAcert.ToArray();
            }
            return null;
        }

        [Authorize]
        public async Task<PartialViewResult> CheckWin()
        {
            var usrId = new UsrMethod().CompareIdUsrCookieToDB(Claim);

            // If user win a Line or Bingo put info on UserData identity.
            ClaimsIdentity? i = Claim.Identities.FirstOrDefault();
            string? winning = User.Identities.FirstOrDefault()!.Claims.ElementAt(5).Value!;
            var rule = new SaloonRule().IsLineWin(usrId, winning);
            if (rule.IsLine)
            {
                var actuaWin = i!.Claims.ElementAt(5);
                i.RemoveClaim(actuaWin);
                var win = new Claim(ClaimTypes.UserData, "Line");
                i!.AddClaim(win);
            }
            else if (rule.IsBingo && winning == "Line")
            {
                var actualwin = i!.Claims.ElementAt(5);
                i.RemoveClaim(actualwin);
                var win = new Claim(ClaimTypes.UserData, "Bingo");
                i!.AddClaim(win);
            }
            var principal = new ClaimsPrincipal(i!);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties());
            return PartialView("_Winning", rule);
        }

        [Authorize]
        public async Task<IActionResult> NewGame()
        {
            try
            {
                //Disable playing Tickets.
                new SaloonRule().DisableT(Claim);

                // Restart Cookie for New Game
                ClaimsIdentity? i = Claim.Identities.FirstOrDefault();
                var actualRole = i!.Claims.ElementAt(3);
                i.RemoveClaim(actualRole);
                var newRole = new Claim(ClaimTypes.Role, "Player");
                i!.AddClaim(newRole);
                var rsa = i!.Claims.ElementAt(3);
                i.RemoveClaim(rsa);
                var userData = i!.Claims.ElementAt(3);
                i.RemoveClaim(userData);
                var principal = new ClaimsPrincipal(i);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties());
                return RedirectToAction("Saloon");
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }
        
        [Authorize]
        public IActionResult History()
        {
            var usrId = new UsrMethod().CompareIdUsrCookieToDB(Claim);
            var rule = new SaloonRule().GetTHistory(usrId);
            return View(rule);
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            new SaloonRule().DisableT(Claim); //Disable playing Tickets.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error");
        }
    }
}