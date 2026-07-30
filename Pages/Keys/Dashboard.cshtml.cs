using KeyManagment.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace KeyManagment.Pages.Keys
{
    [Authorize(Roles = "Admin,SecurityOfficer")]
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

        // فهرست ساختمان‌ها برای فیلتر جدول
        public List<string> BuildingNames { get; set; } = new();

        // داده‌های چارت‌ها (به‌صورت JSON برای مصرف مستقیم در Chart.js)
        public string StatusChartJson { get; set; } = "{}";
        public string BuildingChartJson { get; set; } = "{}";
        public string DeptChartJson { get; set; } = "{}";
        public string TrendChartJson { get; set; } = "{}";

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
                .Take(8)
                .ToListAsync();

            BuildingNames = await _db.Keys
                .Select(k => k.Building.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();

            // ---- چارت وضعیت کلی کلیدها ----
            var jsonOptions = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            StatusChartJson = JsonSerializer.Serialize(new
            {
                labels = new[] { "موجود", "خارج‌شده (در مهلت)", "منقضی شده" },
                values = new[] { AvailableKeys, ActiveHandovers.Count, ExpiredKeys }
            }, jsonOptions);

            // ---- چارت کلیدهای خارج‌شده به تفکیک ساختمان ----
            var byBuilding = allActive
                .GroupBy(h => h.Key.Building.Name)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            BuildingChartJson = JsonSerializer.Serialize(new
            {
                labels = byBuilding.Select(g => g.Label),
                values = byBuilding.Select(g => g.Count)
            }, jsonOptions);

            // ---- چارت کلیدهای خارج‌شده به تفکیک واحد سازمانی ----
            var byDept = allActive
                .GroupBy(h => string.IsNullOrWhiteSpace(h.ReceiverDepartment) ? "نامشخص" : h.ReceiverDepartment)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(8)
                .ToList();

            DeptChartJson = JsonSerializer.Serialize(new
            {
                labels = byDept.Select(g => g.Label),
                values = byDept.Select(g => g.Count)
            }, jsonOptions);

            // ---- روند تحویل کلید در ۷ روز اخیر ----
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i))
                .ToList();

            var handoversLast7 = await _db.KeyHandovers
                .Where(h => h.CheckoutTime >= DateTime.Today.AddDays(-6))
                .Select(h => h.CheckoutTime)
                .ToListAsync();

            var trend = last7Days.Select(d => new
            {
                Label = d.ToString("MM/dd"),
                Count = handoversLast7.Count(c => c.Date == d)
            }).ToList();

            TrendChartJson = JsonSerializer.Serialize(new
            {
                labels = trend.Select(t => t.Label),
                values = trend.Select(t => t.Count)
            }, jsonOptions);
        }
    }
}
