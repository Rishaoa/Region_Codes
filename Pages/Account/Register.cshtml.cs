using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RegionCodeCollector.Data;
using RegionCodeCollector.Models;
using RegionCodeCollector.Services;

namespace RegionCodeCollector.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RegisterModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public RegisterInputModel Input { get; set; } = new();

        public class RegisterInputModel
        {
            [Required(ErrorMessage = "Введите логин")]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 50 символов")]
            [Display(Name = "Логин")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Введите email")]
            [EmailAddress(ErrorMessage = "Введите корректный email")]
            [StringLength(100, ErrorMessage = "Email не должен быть длиннее 100 символов")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Введите пароль")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть минимум 6 символов")]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Повторите пароль")]
            [DataType(DataType.Password)]
            [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
            [Display(Name = "Повтор пароля")]
            public string ConfirmPassword { get; set; } = string.Empty;
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

            string username = Input.Username.Trim();
            string email = Input.Email.Trim().ToLower();

            bool usernameExists = await _context.Users
                .AnyAsync(user => user.Username == username);

            if (usernameExists)
            {
                ModelState.AddModelError(string.Empty, "Пользователь с таким логином уже существует.");
                return Page();
            }

            bool emailExists = await _context.Users
                .AnyAsync(user => user.Email == email);

            if (emailExists)
            {
                ModelState.AddModelError(string.Empty, "Пользователь с таким email уже существует.");
                return Page();
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = PasswordService.HashPassword(Input.Password),
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);

            return RedirectToPage("/MyRegions/Index");
        }
    }
}