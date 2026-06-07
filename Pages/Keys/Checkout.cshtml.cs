using KeyManagment.Data;
using KeyManagment.Models;
using KeyManagment.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeyManagment.Pages.Keys
{
    [Authorize(Roles = "Guard,Admin")]
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
        public List<IdentityUser> AllUsers { get; set; } = new();

        [BindProperty] public int SelectedKeyId { get; set; }
        [BindProperty] public string SelectedUserId { get; set; } = string.Empty;
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

            AllUsers = _userManager.Users.ToList();
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

            var receiver = await _userManager.FindByIdAsync(SelectedUserId);
            if (receiver == null)
            {
                ErrorMessage = "کاربر یافت نشد.";
                await OnGetAsync();
                return Page();
            }

            var guard = await _userManager.GetUserAsync(User);
            var guardClaims = await _userManager.GetClaimsAsync(guard!);
            var receiverClaims = await _userManager.GetClaimsAsync(receiver);

            var handover = new KeyHandover
            {
                KeyId = SelectedKeyId,
                ReceiverId = SelectedUserId,
                ReceiverName = receiverClaims
                    .FirstOrDefault(c => c.Type == "FullName")?.Value
                    ?? receiver.Email ?? "",
                ReceiverDepartment = receiverClaims
                    .FirstOrDefault(c => c.Type == "Department")?.Value ?? "",
                GuardId = guard!.Id,
                GuardName = guardClaims
                    .FirstOrDefault(c => c.Type == "FullName")?.Value
                    ?? guard.Email ?? "",
                CheckoutTime = DateTime.Now,
                Notes = Notes
            };

            key.IsAvailable = false;
            _db.KeyHandovers.Add(handover);
            await _db.SaveChangesAsync();

            SuccessMessage = $"کلید {key.KeyCode} با موفقیت تحویل داده شد.";
            await OnGetAsync();
            return Page();
        }
    }
}