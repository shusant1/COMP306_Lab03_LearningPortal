
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using COMP306_Lab03_LearningPortal.Data;
using COMP306_Lab03_LearningPortal.Models;

namespace COMP306_Lab03_LearningPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly LearningPortalContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(
            LearningPortalContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // Registration page
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // Register new user
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string username = model.Username.Trim();
            string email = model.Email.Trim().ToLowerInvariant();

            bool exists = await _context.Users.AnyAsync(u =>
                u.Username == username || u.Email == email);

            if (exists)
            {
                ModelState.AddModelError("", "Username or email already exists.");
                return View(model);
            }

            User user = new User
            {
                Username = username,
                Email = email,
                Role = "Student"
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Registration successful. Please log in.";

            return RedirectToAction("Login");
        }

        // Login page
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // Login user
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string login = model.UsernameOrEmail.Trim();

            User? user = await _context.Users.FirstOrDefaultAsync(
                u => u.Username == login || u.Email == login);

            // The original sample users cannot log in.
            if (user == null || user.PasswordHash == "SEED_ONLY")
            {
                ModelState.AddModelError("", "Invalid login details.");
                return View(model);
            }

            var result = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid login details.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        // Logout user
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }
    }
}
