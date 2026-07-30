using ClosedXML.Excel;
using KeyManagment.Data;
using KeyManagment.Helpers;
using KeyManagment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using KeyManagment.Helpers;

namespace KeyManagment.Pages.Keys
{
    [Authorize(Roles = "Admin")]
    public class HistoryModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public HistoryModel(ApplicationDbContext db) => _db = db;

        public List<KeyHandover> Handovers { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string? SearchName { get; set; }
        [BindProperty(SupportsGet = true)] public string? SearchKeyCode { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
        [BindProperty(SupportsGet = true)] public string? StatusFilter { get; set; }

        public async Task OnGetAsync()
        {
            Handovers = await GetFilteredQuery().ToListAsync();
        }

        public async Task<IActionResult> OnGetExportAsync()
        {
            var data = await GetFilteredQuery().ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("تاریخچه کلیدها");

            // هدر
            var headers = new[]
            {
    "کد کلید", "اتاق", "ساختمان", "طبقه",
    "تحویل‌گیرنده", "واحد", "حراست",
    "زمان تحویل", "زمان بازگشت", "وضعیت", "توضیحات", "گزارش بازگشت"
};

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(1, i + 1).Value = headers[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
                ws.Cell(1, i + 1).Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#0d6efd");
                ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
            }

            // داده‌ها
            for (int i = 0; i < data.Count; i++)
            {
                var h = data[i];
                var row = i + 2;
                ws.Cell(row, 1).Value = h.Key.KeyCode;
                ws.Cell(row, 2).Value = h.Key.RoomName;
                ws.Cell(row, 3).Value = h.Key.Building.Name;
                ws.Cell(row, 4).Value = h.Key.Floor;
                ws.Cell(row, 5).Value = h.ReceiverName;
                ws.Cell(row, 6).Value = h.ReceiverDepartment;
                ws.Cell(row, 7).Value = h.GuardName;
                ws.Cell(row, 8).Value = h.CheckoutTime.ToShamsiWithTime();
                ws.Cell(row, 9).Value = h.ReturnTime.ToShamsiWithTime() ?? "—";
                ws.Cell(row, 10).Value = h.ReturnTime == null ? "خارج" : "بازگشت";
                ws.Cell(row, 11).Value = h.Notes ?? "";
                ws.Cell(row, 12).Value = h.ReturnNotes ?? "";

            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"keys_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private IQueryable<KeyHandover> GetFilteredQuery()
        {
            var query = _db.KeyHandovers
                .Include(h => h.Key).ThenInclude(k => k.Building)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchName))
                query = query.Where(h =>
                    h.ReceiverName.Contains(SearchName) ||
                    h.GuardName.Contains(SearchName));

            if (!string.IsNullOrEmpty(SearchKeyCode))
                query = query.Where(h =>
                    h.Key.KeyCode.Contains(SearchKeyCode));

            if (FromDate.HasValue)
                query = query.Where(h => h.CheckoutTime >= FromDate.Value);

            if (ToDate.HasValue)
                query = query.Where(h =>
                    h.CheckoutTime <= ToDate.Value.AddDays(1));

            if (StatusFilter == "out")
                query = query.Where(h => h.ReturnTime == null);
            else if (StatusFilter == "returned")
                query = query.Where(h => h.ReturnTime != null);

            return query.OrderByDescending(h => h.CheckoutTime);
        }
    }
}