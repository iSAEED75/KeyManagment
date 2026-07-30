using ClosedXML.Excel;
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
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> ImportErrors { get; set; } = new();

        [BindProperty] public string NewPersonnelCode { get; set; } = string.Empty;
        [BindProperty] public string NewPassword { get; set; } = string.Empty;
        [BindProperty] public string NewFullName { get; set; } = string.Empty;
        [BindProperty] public string NewRole { get; set; } = string.Empty;
        [BindProperty] public string NewDepartment { get; set; } = string.Empty;
        [BindProperty] public string NewPhone { get; set; } = string.Empty;
        [BindProperty] public IFormFile? ExcelFile { get; set; }

        public async Task OnGetAsync()
        {
            await LoadUsersAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (await _userManager.FindByNameAsync(NewPersonnelCode) != null)
            {
                ErrorMessage = $"شماره پرسنلی {NewPersonnelCode} قبلاً ثبت شده.";
                await LoadUsersAsync();
                return Page();
            }

            var user = new IdentityUser
            {
                UserName = NewPersonnelCode,
                Email = $"{NewPersonnelCode}@nouri.ir",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, NewPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, NewRole);
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", NewFullName));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Department", NewDepartment));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("PersonnelCode", NewPersonnelCode));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Phone", NewPhone));
                SuccessMessage = $"کاربر {NewFullName} با شماره پرسنلی {NewPersonnelCode} اضافه شد.";
            }
            else
            {
                ErrorMessage = string.Join(" — ", result.Errors.Select(e => e.Description));
            }

            await LoadUsersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostImportAsync()
        {
            if (ExcelFile == null || ExcelFile.Length == 0)
            {
                ErrorMessage = "لطفاً یک فایل اکسل انتخاب کنید.";
                await LoadUsersAsync();
                return Page();
            }

            int successCount = 0;
            ImportErrors = new List<string>();

            using var stream = new MemoryStream();
            await ExcelFile.CopyToAsync(stream);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed()?.RowsUsed().Skip(1); // رد کردن سطر اول (هدر)

            if (rows == null)
            {
                ErrorMessage = "فایل اکسل خالی است.";
                await LoadUsersAsync();
                return Page();
            }

            foreach (var row in rows)
            {
                var personnelCode = row.Cell(1).GetString().Trim();
                var fullName = row.Cell(2).GetString().Trim();
                var phone = row.Cell(3).GetString().Trim();
                var department = row.Cell(4).GetString().Trim();
                var role = row.Cell(5).GetString().Trim();
                var password = row.Cell(6).GetString().Trim();

                if (string.IsNullOrEmpty(personnelCode) || string.IsNullOrEmpty(password))
                {
                    ImportErrors.Add($"سطر {row.RowNumber()}: شماره پرسنلی یا رمز عبور خالی است.");
                    continue;
                }

                // نقش پیش‌فرض
                if (string.IsNullOrEmpty(role)) role = "User";

                if (await _userManager.FindByNameAsync(personnelCode) != null)
                {
                    ImportErrors.Add($"سطر {row.RowNumber()}: شماره پرسنلی {personnelCode} قبلاً ثبت شده.");
                    continue;
                }

                var user = new IdentityUser
                {
                    UserName = personnelCode,
                    Email = $"{personnelCode}@nouri.ir",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role);
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", fullName));
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Department", department));
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("PersonnelCode", personnelCode));
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("Phone", phone));
                    successCount++;
                }
                else
                {
                    var errs = string.Join("، ", result.Errors.Select(e => e.Description));
                    ImportErrors.Add($"سطر {row.RowNumber()} ({personnelCode}): {errs}");
                }
            }

            SuccessMessage = $"{successCount} کاربر با موفقیت وارد شد.";
            await LoadUsersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == userId)
            {
                ErrorMessage = "نمی‌توانید حساب خودتان را حذف کنید.";
                await LoadUsersAsync();
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    SuccessMessage = "کاربر با موفقیت حذف شد.";
                else
                    ErrorMessage = "خطا در حذف کاربر.";
            }

            await LoadUsersAsync();
            return Page();
        }

        public IActionResult OnPostDownloadTemplateAsync()
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("کاربران");

            // هدر
            ws.Cell(1, 1).Value = "شماره پرسنلی";
            ws.Cell(1, 2).Value = "نام کامل";
            ws.Cell(1, 3).Value = "شماره تماس";
            ws.Cell(1, 4).Value = "واحد";
            ws.Cell(1, 5).Value = "نقش";
            ws.Cell(1, 6).Value = "رمز عبور";

            // سطر نمونه
            ws.Cell(2, 1).Value = "12345";
            ws.Cell(2, 2).Value = "علی محمدی";
            ws.Cell(2, 3).Value = "09171234567";
            ws.Cell(2, 4).Value = "واحد فنی";
            ws.Cell(2, 5).Value = "User";
            ws.Cell(2, 6).Value = "Pass@1234";

            // توضیح نقش‌ها
            ws.Cell(4, 1).Value = "نقش‌های مجاز:";
            ws.Cell(4, 2).Value = "User = کارمند عادی";
            ws.Cell(5, 2).Value = "Guard = حراست";
            ws.Cell(6, 2).Value = "Admin = مدیر";

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "users_template.xlsx");
        }

        private async Task LoadUsersAsync()
        {
            Users = new List<UserViewModel>();
            foreach (var user in _userManager.Users.ToList())
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);
                Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    PersonnelCode = user.UserName ?? "",
                    FullName = claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "—",
                    Department = claims.FirstOrDefault(c => c.Type == "Department")?.Value ?? "—",
                    Phone = claims.FirstOrDefault(c => c.Type == "Phone")?.Value ?? "—",
                    Role = roles.FirstOrDefault() ?? "بدون نقش"
                });
            }
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string PersonnelCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}