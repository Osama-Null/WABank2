using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WABank.Models;
using WABank.Models.ViewModels;

namespace WABank.Controllers
{
    public class AccountController : Controller
    {
        #region InjectedServices
        private UserManager<AppUser> _userManager;
        private SignInManager<AppUser> _signInManager;
        private RoleManager<IdentityRole> _roleManager;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> SignInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = SignInManager;
            _roleManager = roleManager;
        }
        #endregion
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult RegisterLogin()
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles, "RoleId", "Name");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterLoginViewModel model)
        {
            ModelState.Remove("LoginVM");
            if (!ModelState.IsValid)
            {
                Console.WriteLine(ModelState);
                ViewBag.Roles = new SelectList(_roleManager.Roles, "RoleId", "Name");
                return View("RegisterLogin", model);
            }

            AppUser user = new AppUser
            {
                UserName = model.RegisterVM.Email,
                Email = model.RegisterVM.Email,
                PhoneNumber = model.RegisterVM.Mobile,
            };
            var result = await _userManager.CreateAsync(user, model.RegisterVM.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return View("RegisterLogin", model);
            }
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("RegisterLogin");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(RegisterLoginViewModel model)
        {
            ModelState.Remove("RegisterVM");
            if (!ModelState.IsValid)
            {
                return View("RegisterLogin", model);
            }
            var result = await _signInManager.PasswordSignInAsync(model.LoginVM.Email, model.LoginVM.Password, model.LoginVM.RememberMe, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("LoginError", "Invalid email or password.");
                return View("RegisterLogin", model);
            }
            return RedirectToAction("RegisterLogin");
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            Console.WriteLine("username" + user.Email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var transactions = await _context.Transactions
                                             .Where(t => t.UserId == user.Id)
                                             .ToListAsync();

            return View(transactions);
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Profile");
        }
        [HttpPost]
        public async Task<IActionResult> LogoutPost()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("RegisterLogin");
        }

        //==================DEPOSIT==================\\
        public async Task<IActionResult> Deposit(int? id)
        { 
            if (id == null || id.ToString() == string.Empty)
            {
                return RedirectToAction("Deposit");
            }

            var acco = await _userManager.FindByIdAsync(id);
            if (acco == null || string.IsNullOrEmpty(acco.Email))
            {
                return RedirectToAction("Deposit");
            }
            TransactionViewModel model = new TransactionViewModel
            {
                UserId = acco.UserId,
                AmountVM = acco.Amount,
                DateVM = DateTime.Now,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Deposit(TransactionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // The old role in roleManager
            var account = await _context.Accounts.FindAsync(model.UserId);
            if (account == null)
            {
                return RedirectToAction("Account");
            }
            account.Amount += model.AmountVM;

            //var transaction = new Transaction
            //{
            //    Transaction = Guid.NewGuid().ToString(), // Generate a unique transaction number
            //    UserId = model.UserId,
            //    Amount = model.AmountVM,
            //    Date = DateTime.UtcNow,
            //    Type = "Deposit"
            //};

            //await _context.Transactions.AddAsync(transaction); // adding to the data base
            //_context.Accounts.Update(account);
            //await _context.SaveChangesAsync();

            return RedirectToAction("AccountDetails", new { id = model.UserId });

            // Assign the edited (updated) data to the old roleManager
            account.Amount = model.AmountVM;
            //account. = model.AmountVM;
            // Update the role manager since we replaced its value name with the model.name from the EditRoleViewModel
            //var result = _context.Accounts.Update(account);
            //if (!result.Succeeded)
            //{
            //    foreach (var error in result.Errors)
            //    {
            //        ModelState.AddModelError(error.Code, error.Description);
            //    }
            //    return View(model);
            //}
            return RedirectToAction("RolesList");
        }
        //==================WITHDRAW==================\\
        public IActionResult Withdraw()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Withdraw(int? id)
        {
            return RedirectToAction("Login");
        }
        //==================TRANSFER==================\\
        public IActionResult Transfer()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Transfer(int? id)
        {
            return RedirectToAction("Login");
        }
    }
}
