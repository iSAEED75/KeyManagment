using KeyManagment.Data;
using KeyManagment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
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
        [BindProperty] public IFormFile? ExcelFile { get; set; }
        public string? ImportMessage { get; set; }
        public List<string> ImportErrors { get; set; } = new();

        // دانلود فایل نمونه
        public IActionResult OnGetDownloadTemplate()
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("کلیدها");

            // هدر
            ws.Cell(1, 1).Value = "KeyCode";
            ws.Cell(1, 2).Value = "RoomName";
            ws.Cell(1, 3).Value = "Floor";
            ws.Cell(1, 4).Value = "BuildingName";

            // استایل هدر
            var header = ws.Range("A1:D1");
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#0d6efd");
            header.Style.Font.FontColor = XLColor.White;

            // داده نمونه
            ws.Cell(2, 1).Value = "A1-F1-R101";
            ws.Cell(2, 2).Value = "اتاق مدیریت";
            ws.Cell(2, 3).Value = 1;
            ws.Cell(2, 4).Value = "ساختمان الف";

            ws.Cell(3, 1).Value = "A1-F2-R201";
            ws.Cell(3, 2).Value = "اتاق IT";
            ws.Cell(3, 3).Value = 2;
            ws.Cell(3, 4).Value = "ساختمان الف";

            ws.Cell(4, 1).Value = "B1-F1-R101";
            ws.Cell(4, 2).Value = "انبار";
            ws.Cell(4, 3).Value = 1;
            ws.Cell(4, 4).Value = "ساختمان ب";

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "template_keys.xlsx");
        }

        // آپلود و پردازش فایل Excel
        public async Task<IActionResult> OnPostImportAsync()
        {
            if (ExcelFile == null || ExcelFile.Length == 0)
            {
                ImportMessage = "فایلی انتخاب نشده.";
                await OnGetAsync();
                return Page();
            }

            int successCount = 0;
            int skipCount = 0;

            using var stream = new MemoryStream();
            await ExcelFile.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheet(1);
            var rows = ws.RowsUsed().Skip(1); // رد کردن هدر

            foreach (var row in rows)
            {
                var keyCode = row.Cell(1).GetValue<string>().Trim();
                var roomName = row.Cell(2).GetValue<string>().Trim();
                var floor = row.Cell(3).GetValue<int>();
                var buildingName = row.Cell(4).GetValue<string>().Trim();

                if (string.IsNullOrEmpty(keyCode) || string.IsNullOrEmpty(roomName))
                {
                    ImportErrors.Add($"ردیف {row.RowNumber()}: کد کلید یا نام اتاق خالیه.");
                    continue;
                }

                // اگه کلید با این کد قبلاً هست رد کن
                if (await _db.Keys.AnyAsync(k => k.KeyCode == keyCode))
                {
                    skipCount++;
                    ImportErrors.Add($"کلید {keyCode} قبلاً وجود دارد — رد شد.");
                    continue;
                }

                // ساختمان رو پیدا کن یا بساز
                var building = await _db.Buildings
                    .FirstOrDefaultAsync(b => b.Name == buildingName);

                if (building == null)
                {
                    building = new Building { Name = buildingName };
                    _db.Buildings.Add(building);
                    await _db.SaveChangesAsync();
                }

                var key = new Key
                {
                    KeyCode = keyCode,
                    RoomName = roomName,
                    Floor = floor,
                    BuildingId = building.Id,
                    IsAvailable = true
                };

                _db.Keys.Add(key);
                successCount++;
            }

            await _db.SaveChangesAsync();
            ImportMessage = $"✅ {successCount} کلید با موفقیت وارد شد. {skipCount} کلید تکراری رد شد.";

            await OnGetAsync();
            return Page();
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