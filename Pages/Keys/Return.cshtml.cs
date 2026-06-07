using KeyManagment.Data;
using KeyManagment.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeyManagment.Pages.Keys
{
    [Authorize(Roles = "Guard,Admin")]
    public class ReturnModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public ReturnModel(ApplicationDbContext db) => _db = db;

        public List<KeyManagment.Models.KeyHandover> ActiveHandovers { get; set; } = new();
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            ActiveHandovers = await _db.KeyHandovers
                .Include(h => h.Key)
                    .ThenInclude(k => k.Building)
                .Where(h => h.ReturnTime == null)
                .OrderBy(h => h.CheckoutTime)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostReturnAsync(int handoverId)
        {
            var handover = await _db.KeyHandovers
                .Include(h => h.Key)
                .FirstOrDefaultAsync(h => h.Id == handoverId);

            if (handover != null)
            {
                handover.ReturnTime = DateTime.Now;
                handover.Key.IsAvailable = true;
                await _db.SaveChangesAsync();
                SuccessMessage = $"کلید {handover.Key.KeyCode} بازگشت داده شد.";
            }

            await OnGetAsync();
            return Page();
        }
    }
}