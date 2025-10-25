using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PhamVanTung.SachOnline.Models;
using PhamVanTung.SachOnline.Models.ViewModels;

public class UserController : Controller
{
    private readonly SachOnlineEntities db = new SachOnlineEntities();

    private static string Sha256Hex(string input)
    {
        using (var sha = SHA256.Create())
        {
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }

    private static string Sha256Base64(string input)
    {
        using (var sha = SHA256.Create())
        {
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }

    private static bool IsHex(string value)
    {
        foreach (var c in value)
        {
            if (!Uri.IsHexDigit(c))
            {
                return false;
            }
        }

        return true;
    }

    private static bool VerifyPassword(string rawInput, string stored)
    {
        if (string.IsNullOrWhiteSpace(stored))
        {
            return false;
        }

        stored = stored.Trim();

        if (stored.StartsWith("$2a$") || stored.StartsWith("$2b$") || stored.StartsWith("$2y$"))
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(rawInput, stored);
            }
            catch
            {
                return false;
            }
        }

        if (stored.Length == 64 && IsHex(stored))
        {
            return string.Equals(Sha256Hex(rawInput), stored, StringComparison.OrdinalIgnoreCase);
        }

        if (stored.Length == 43 || stored.Length == 44)
        {
            var calc = Sha256Base64(rawInput);
            if (calc.Equals(stored, StringComparison.Ordinal) ||
                calc.TrimEnd('=').Equals(stored.TrimEnd('='), StringComparison.Ordinal))
            {
                return true;
            }
        }

        return rawInput == stored;
    }

    [HttpGet]
    public ActionResult DangKy()
    {
        return View(new DangKyVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult DangKy(DangKyVM model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (db.KHACHHANGs.Any(x => x.TaiKhoan == model.TaiKhoan))
        {
            ModelState.AddModelError(nameof(model.TaiKhoan), "Tai khoan da ton tai!");
            return View(model);
        }

        var kh = new KHACHHANG
        {
            HoTen = model.HoTen?.Trim(),
            TaiKhoan = model.TaiKhoan?.Trim(),
            MatKhau = model.MatKhau?.Trim(),
            Email = model.Email?.Trim(),
            DienThoai = string.IsNullOrWhiteSpace(model.DienThoai) ? null : model.DienThoai.Trim(),
            DiaChi = string.IsNullOrWhiteSpace(model.DiaChi) ? null : model.DiaChi.Trim(),
            NgaySinh = model.NgaySinh ?? DateTime.Now
        };

        db.KHACHHANGs.Add(kh);
        db.SaveChanges();

        TempData["ThongBao"] = "Dang ky thanh cong! Ban co the dang nhap ngay.";
        return RedirectToAction("DangNhap");
    }

    [HttpGet]
    public ActionResult DangNhap(string returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult DangNhap(string TaiKhoan, string MatKhau, bool RememberMe = false, string returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(TaiKhoan) || string.IsNullOrWhiteSpace(MatKhau))
        {
            ModelState.AddModelError("", "Vui long nhap day du tai khoan va mat khau.");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        var kh = db.KHACHHANGs.FirstOrDefault(x => x.TaiKhoan.ToLower() == TaiKhoan.Trim().ToLower());
        if (kh == null || !VerifyPassword(MatKhau, kh.MatKhau))
        {
            ModelState.AddModelError("", "Tai khoan hoac mat khau khong dung.");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        FormsAuthentication.SetAuthCookie(kh.TaiKhoan, RememberMe);
        Session["TaiKhoan"] = kh;
        Session["UserName"] = kh.TaiKhoan;

        if (RememberMe)
        {
            var cookie = new HttpCookie("RememberMe", kh.TaiKhoan)
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true
            };
            Response.Cookies.Add(cookie);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "PhamVanTungSachOnline");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult DangXuat()
    {
        FormsAuthentication.SignOut();
        Session.Clear();

        if (Request.Cookies["RememberMe"] != null)
        {
            var c = new HttpCookie("RememberMe") { Expires = DateTime.Now.AddDays(-1) };
            Response.Cookies.Add(c);
        }

        return RedirectToAction("Index", "PhamVanTungSachOnline");
    }

    [HttpGet]
    public ActionResult DangNhapCookie()
    {
        var cookie = Request.Cookies["RememberMe"];
        if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value))
        {
            TempData["ThongBao"] = "Khong tim thay cookie dang nhap hop le.";
            return RedirectToAction("DangNhap");
        }

        var user = db.KHACHHANGs.FirstOrDefault(x => x.TaiKhoan == cookie.Value);
        if (user == null)
        {
            TempData["ThongBao"] = "Cookie khong hop le hoac tai khoan da bi xoa.";
            return RedirectToAction("DangNhap");
        }

        FormsAuthentication.SetAuthCookie(user.TaiKhoan, true);
        Session["TaiKhoan"] = user;
        Session["UserName"] = user.TaiKhoan;

        TempData["ThongBao"] = "Dang nhap tu dong thanh cong!";
        return RedirectToAction("Index", "PhamVanTungSachOnline");
    }

    [Authorize]
    public ActionResult ThongTin()
    {
        var kh = db.KHACHHANGs.FirstOrDefault(x => x.TaiKhoan == User.Identity.Name);
        if (kh == null)
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("DangNhap");
        }

        return View(kh);
    }
}
