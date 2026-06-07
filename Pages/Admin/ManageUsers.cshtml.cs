using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeyManagment.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ManageUsersModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManageUsersModel(UserManager<IdentityUser> userManager,
                                RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<UserViewModel> Users { get; set; } = new();

        [BindProperty] public string NewEmail { get; set; } = string.Empty;
        [BindProperty] public string NewPassword { get; set; } = string.Empty;
        [BindProperty] public string NewFullName { get; set; } = string.Empty;
        [BindProperty] public string NewRole { get; set; } = string.Empty;
        [BindProperty] public string NewDepartment { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            foreach (var user in _userManager.Users.ToList())
            {
                var roles = await _userManager.GetRolesAsync(user);
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FullName = user.UserName ?? "",
                    Role = roles.FirstOrDefault() ?? "بدون نقش"
                });
            }
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            var user = new IdentityUser
            {
                UserName = NewEmail,
                Email = NewEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, NewPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, NewRole);
                // ذخیره نام کامل در claims
                await _userManager.AddClaimAsync(user,
                    new System.Security.Claims.Claim("FullName", NewFullName));
                await _userManager.AddClaimAsync(user,
                    new System.Security.Claims.Claim("Department", NewDepartment));
            }

            return RedirectToPage();
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}