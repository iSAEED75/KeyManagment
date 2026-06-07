using KeyManagment.Data;
using KeyManagment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeyManagment.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ManageKeysModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public ManageKeysModel(ApplicationDbContext db) => _db = db;

        public List<Key> Keys { get; set; } = new();
        public List<Building> Buildings { get; set; } = new();

        [BindProperty] public string NewKeyCode { get; set; } = string.Empty;
        [BindProperty] public string NewRoomName { get; set; } = string.Empty;
        [BindProperty] public int NewFloor { get; set; }
        [BindProperty] public int NewBuildingId { get; set; }

        [BindProperty] public string NewBuildingName { get; set; } = string.Empty;
        [BindProperty] public string? NewBuildingDesc { get; set; }

        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            Keys = await _db.Keys
                .Include(k => k.Building)
                .OrderBy(k => k.Building.Name)
                .ThenBy(k => k.Floor)
                .ToListAsync();

            Buildings = await _db.Buildings.ToListAsync();
        }

        // افزودن کلید جدید
        public async Task<IActionResult> OnPostAddKeyAsync()
        {
            var key = new Key
            {
                KeyCode = NewKeyCode,
                RoomName = NewRoomName,
                Floor = NewFloor,
                BuildingId = NewBuildingId,
                IsAvailable = true
            };
            _db.Keys.Add(key);
            await _db.SaveChangesAsync();
            SuccessMessage = $"کلید {NewKeyCode} اضافه شد.";
            await OnGetAsync();
            return Page();
        }

        // افزودن ساختمان جدید
        public async Task<IActionResult> OnPostAddBuildingAsync()
        {
            var building = new Building
            {
                Name = NewBuildingName,
                Description = NewBuildingDesc
            };
            _db.Buildings.Add(building);
            await _db.SaveChangesAsync();
            SuccessMessage = $"ساختمان {NewBuildingName} اضافه شد.";
            await OnGetAsync();
            return Page();
        }

        // حذف کلید
        public async Task<IActionResult> OnPostDeleteKeyAsync(int keyId)
        {
            var key = await _db.Keys.FindAsync(keyId);
            if (key != null && key.IsAvailable)
            {
                _db.Keys.Remove(key);
                await _db.SaveChangesAsync();
                SuccessMessage = "کلید حذف شد.";
            }
            else
            {
                SuccessMessage = "کلید در دست کاربر است و قابل حذف نیست.";
            }
            await OnGetAsync();
            return Page();
        }
    }
}