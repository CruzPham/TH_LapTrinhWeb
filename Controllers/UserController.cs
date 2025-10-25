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

        var hashedPassword = Sha256Base64(model.MatKhau);
        var kh = new KHACHHANG
        {
            HoTen = model.HoTen?.Trim(),
            TaiKhoan = model.TaiKhoan?.Trim(),
            MatKhau = hashedPassword,
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
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            returnUrl = Url.Action("Index", "PhamVanTungSachOnline");
        }

        ViewBag.ReturnUrl = returnUrl;
        var model = new DangNhapVM { ReturnUrl = returnUrl };

        var savedUser = Request.Cookies["RememberUserName"];
        var savedPass = Request.Cookies["RememberPassword"];
        if (savedUser != null && savedPass != null)
        {
            model.TaiKhoan = savedUser.Value;
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(savedPass.Value));
                model.MatKhau = decoded;
            }
            catch
            {
                model.MatKhau = string.Empty;
            }
            model.RememberMe = true;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult DangNhap(DangNhapVM model, string returnUrl = null)
    {
        returnUrl = returnUrl ?? model.ReturnUrl;
        if (!ModelState.IsValid)
        {
            ViewBag.ReturnUrl = returnUrl;
            model.ReturnUrl = returnUrl;
            return View(model);
        }

        var kh = db.KHACHHANGs.FirstOrDefault(x => x.TaiKhoan.ToLower() == model.TaiKhoan.Trim().ToLower());
        if (kh == null || !VerifyPassword(model.MatKhau, kh.MatKhau))
        {
            ModelState.AddModelError("", "Tai khoan hoac mat khau khong dung.");
            ViewBag.ReturnUrl = returnUrl;
            model.ReturnUrl = returnUrl;
            return View(model);
        }

        FormsAuthentication.SetAuthCookie(kh.TaiKhoan, model.RememberMe);
        Session["TaiKhoan"] = kh;
        Session["UserName"] = kh.TaiKhoan;

        if (model.RememberMe)
        {
            var cookie = new HttpCookie("RememberMe", kh.TaiKhoan)
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true
            };
            Response.Cookies.Add(cookie);

            Response.Cookies.Add(new HttpCookie("RememberUserName", model.TaiKhoan)
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = false
            });

            var encodedPw = Convert.ToBase64String(Encoding.UTF8.GetBytes(model.MatKhau));
            Response.Cookies.Add(new HttpCookie("RememberPassword", encodedPw)
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = false
            });
        }
        else
        {
            Response.Cookies.Add(new HttpCookie("RememberMe") { Expires = DateTime.Now.AddDays(-1) });
            Response.Cookies.Add(new HttpCookie("RememberUserName") { Expires = DateTime.Now.AddDays(-1) });
            Response.Cookies.Add(new HttpCookie("RememberPassword") { Expires = DateTime.Now.AddDays(-1) });
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

    [ChildActionOnly]
    public ActionResult LoginLogoutPartial()
    {
        var kh = Session["TaiKhoan"] as KHACHHANG;
        ViewBag.CurrentUrl = Request?.RawUrl;
        return PartialView("~/Views/User/LoginLogoutPartial.cshtml", kh);
    }
}


