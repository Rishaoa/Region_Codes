using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RegionCodeCollector.Data;
using RegionCodeCollector.Services;

namespace RegionCodeCollector.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public class LoginInputModel
        {
            [Required(ErrorMessage = "Введите логин или email")]
            [Display(Name = "Логин или email")]
            public string Login { get; set; } = string.Empty;

            [Required(ErrorMessage = "Введите пароль")]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; } = string.Empty;
        }

        public IActionResult OnGet()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId != null)
            {
                return RedirectToPage("/MyRegions/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            int? currentUserId = HttpContext.Session.GetInt32("UserId");

            if (currentUserId != null)
            {
                return RedirectToPage("/MyRegions/Index");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string login = Input.Login.Trim();
            string emailLogin = login.ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Username == login || user.Email == emailLogin);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Неверный логин, email или пароль.");
                return Page();
            }

            bool passwordIsValid = PasswordService.VerifyPassword(Input.Password, user.PasswordHash);

            if (!passwordIsValid)
            {
                ModelState.AddModelError(string.Empty, "Неверный логин, email или пароль.");
                return Page();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToPage("/MyRegions/Index");
        }
    }
}