using KeyManagment.Data;
using KeyManagment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeyManagment.Pages.Keys
{
    [Authorize(Roles = "Guard,Admin,SecurityOfficer")]
    public class CheckoutModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public CheckoutModel(ApplicationDbContext db,
                             UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<Key> AvailableKeys { get; set; } = new();
        public List<UserViewModel> Users { get; set; } = new();

        [BindProperty] public int SelectedKeyId { get; set; }
        [BindProperty] public string ReceiverName { get; set; } = string.Empty;
        [BindProperty] public string? ReceiverDepartment { get; set; }
        [BindProperty] public double AllowedHours { get; set; } = 8;
        [BindProperty] public string? Notes { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            AvailableKeys = await _db.Keys
                .Include(k => k.Building)
                .Where(k => k.IsAvailable)
                .OrderBy(k => k.Building.Name)
                .ThenBy(k => k.Floor)
                .ToListAsync();

            // لیست کاربران
            foreach (var user in _userManager.Users.ToList())
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var fullName = claims
                    .FirstOrDefault(c => c.Type == "FullName")?.Value
                    ?? user.Email ?? "";
                var department = claims
                    .FirstOrDefault(c => c.Type == "Department")?.Value ?? "";
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    FullName = fullName,
                    Department = department,
                    Email = user.Email ?? ""
                });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var key = await _db.Keys.FindAsync(SelectedKeyId);
            if (key == null || !key.IsAvailable)
            {
                ErrorMessage = "این کلید در دسترس نیست.";
                await OnGetAsync();
                return Page();
            }

            var guard = await _userManager.GetUserAsync(User);
            var guardClaims = await _userManager.GetClaimsAsync(guard!);

            var handover = new KeyHandover
            {
                KeyId = SelectedKeyId,
                ReceiverId = "",
                ReceiverName = ReceiverName,
                ReceiverDepartment = ReceiverDepartment ?? "",
                GuardId = guard!.Id,
                GuardName = guardClaims
                    .FirstOrDefault(c => c.Type == "FullName")?.Value
                    ?? guard.Email ?? "",
                CheckoutTime = DateTime.Now,
                AllowedHours = AllowedHours,
                Notes = Notes
            };

            key.IsAvailable = false;
            _db.KeyHandovers.Add(handover);
            await _db.SaveChangesAsync();

            SuccessMessage = $"کلید {key.KeyCode} با موفقیت به {ReceiverName} تحویل داده شد.";
            await OnGetAsync();
            return Page();
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

    }

}