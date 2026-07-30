using KeyManagment.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeyManagment.Pages.Keys
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public DashboardModel(ApplicationDbContext db) => _db = db;

        public int TotalKeys { get; set; }
        public int AvailableKeys { get; set; }
        public int CheckedOutKeys { get; set; }
        public int ExpiredKeys { get; set; }

        public List<KeyManagment.Models.KeyHandover> ActiveHandovers { get; set; } = new();
        public List<KeyManagment.Models.KeyHandover> ExpiredHandovers { get; set; } = new();
        public List<KeyManagment.Models.KeyHandover> RecentHistory { get; set; } = new();

        public async Task OnGetAsync()
        {
            TotalKeys = await _db.Keys.CountAsync();
            AvailableKeys = await _db.Keys.CountAsync(k => k.IsAvailable);
            CheckedOutKeys = TotalKeys - AvailableKeys;

            var allActive = await _db.KeyHandovers
                .Include(h => h.Key).ThenInclude(k => k.Building)
                .Where(h => h.ReturnTime == null)
                .OrderBy(h => h.CheckoutTime)
                .ToListAsync();

            ExpiredHandovers = allActive.Where(h => h.IsExpired).ToList();
            ActiveHandovers = allActive.Where(h => !h.IsExpired).ToList();
            ExpiredKeys = ExpiredHandovers.Count;

            RecentHistory = await _db.KeyHandovers
                .Include(h => h.Key).ThenInclude(k => k.Building)
                .Where(h => h.ReturnTime != null)
                .OrderByDescending(h => h.ReturnTime)
                .Take(20)
                .ToListAsync();
        }
    }
}