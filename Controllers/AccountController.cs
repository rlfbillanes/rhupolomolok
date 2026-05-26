using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using rhupolomolok.Models;
using Microsoft.AspNetCore.Authorization;

namespace rhupolomolok.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // LOGIN (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN (POST)
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        // LOGOUT
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ACCESS DENIED
        public IActionResult AccessDenied()
        {
            return View();
        }

        // REGISTER (GET) — Admin Only
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Roles = new List<string> { "Administrator", "Contributor" };
            return View();
        }

        // REGISTER (POST) — Admin Only
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string> { "Administrator", "Contributor" };
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.Roles = new List<string> { "Administrator", "Contributor" };
            return View(model);
        }




    }
}
