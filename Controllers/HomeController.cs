using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankAccount.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BankAccount.Controllers
{
    public class HomeController : Controller
    {

        private MyContext dbContext;

        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [Route("processuser")]
        public IActionResult ProcessUser(User user)
        {
        if(ModelState.IsValid)
            {
        // If a User exists with provided email
                if(dbContext.Users.Any(u => u.Email == user.Email))
        {
            // Manually add a ModelState error to the Email field, with provided
            // error message
            ModelState.AddModelError("Email", "Email already in use!");
            
            return Redirect("/");
        }
        else{
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            user.Password = Hasher.HashPassword(user, user.Password);
            dbContext.Users.Add(user);
            dbContext.SaveChanges();
            HttpContext.Session.SetInt32("UserId", user.UserId);
            return Redirect("transactions");
            }
        }
        return Redirect("/");

        }
        [HttpGet]
        [Route("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("processlogin")]

        public IActionResult ProcessLogin(LoginUser login)
        {
            if(ModelState.IsValid)
            {
            // If inital ModelState is valid, query for a user with provided email
            var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == login.LoginEmail);
            // If no user exists with provided email
            if(userInDb == null)
            {
                // Add an error to ModelState and return to View!
                ModelState.AddModelError("Email", "Invalid Email/Password");
                return View("Login");
            }
            else {

            // Initialize hasher object
            var hasher = new PasswordHasher<LoginUser>();
            
            // verify provided password against hash stored in db
            var result = hasher.VerifyHashedPassword(login, userInDb.Password, login.LoginPassword);
            
            // result can be compared to 0 for failure
            if(result == 0)
            {
                ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                return Redirect("Login");
            }
            HttpContext.Session.SetInt32("UserId", userInDb.UserId);
            return Redirect($"transactions/{userInDb.UserId}");

            }
    
        }
        return Redirect("/login");
        }


        [HttpGet]
        [Route("logout")]

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return Redirect("/");
        }

        [HttpGet]
        [Route("transactions/{userid}")]
        public IActionResult Transactions(int userid)
        { 
            if(HttpContext.Session.GetInt32("UserId") == null) {
                return Redirect("/login");
            }
            if(HttpContext.Session.GetInt32("UserId") != userid) {
                int? UserId = HttpContext.Session.GetInt32("UserId");
                return Redirect($"/transactions/{UserId}");
            }
            else
            {
                User DbUser = dbContext.Users.Include(u=>u.UserTransactions).FirstOrDefault( u => u.UserId == userid);
                ViewBag.LoggedInUser=DbUser;
                
            }
                return View();
            }
        [HttpPost]
        [Route("accountactivity/{userId}")]
        public IActionResult AccountActivity(Transaction newtransaction, int userId)
        {
           
                User thisuser=dbContext.Users.Include(u=>u.UserTransactions).FirstOrDefault(u=>u.UserId==userId);
        
                if (newtransaction.Amount > 0)
                    {
                    thisuser.Balance+=newtransaction.Amount;

                    dbContext.Transactions.Add(newtransaction);
                    dbContext.SaveChanges();
                    ViewBag.UserInfo=thisuser;
                
                
                return Redirect($"/transactions/{thisuser.UserId}");

                }
                if (newtransaction.Amount < 0)
                {
                thisuser.Balance-=newtransaction.Amount;


                dbContext.Transactions.Add(newtransaction);
                dbContext.SaveChanges();

                ViewBag.UserInfo=thisuser;
                
                return Redirect($"/transactions/{thisuser.UserId}");
                }



            return Redirect($"/transactions/{thisuser.UserId}");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
