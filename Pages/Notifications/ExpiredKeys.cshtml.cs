using KeyManagment.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeyManagment.Pages.Notifications
{
    [Authorize(Roles = "Admin,SecurityOfficer,Guard")]
    public class ExpiredKeysModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public ExpiredKeysModel(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> OnGetAsync()
        {
            // چون IsExpired یک پراپرتی محاسبه‌شده تو مدل هست، نمی‌شه تو کوئری SQL فیلترش کرد
            var openHandovers = await _db.KeyHandovers
                .Include(h => h.Key).ThenInclude(k => k.Building)
                .Where(h => h.ReturnTime == null)
                .ToListAsync();

            var expired = openHandovers
                .Where(h => h.IsExpired)
                .Select(h => new
                {
                    id = h.Id,
                    keyCode = h.Key.KeyCode,
                    roomName = h.Key.RoomName,
                    buildingName = h.Key.Building.Name,
                    receiverName = h.ReceiverName,
                    expiryTime = h.ExpiryTime
                })
                .ToList();

            return new JsonResult(expired);
        }
    }
}