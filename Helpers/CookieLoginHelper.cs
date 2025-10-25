using System;
using System.Text;
using System.Web;
using System.Web.Security;

namespace PhamVanTung.SachOnline.Models
{
    public static class CookieLoginHelper
    {
        private const string CookieName = "SO_LOGIN";
        private static readonly string[] Purpose = new[] { "PhamVanTung.SachOnline", "SO_LOGIN_COOKIE" };

        public static void Save(string taiKhoanOrEmail, string plainPassword, int days = 7)
        {
            if (string.IsNullOrWhiteSpace(taiKhoanOrEmail) || string.IsNullOrWhiteSpace(plainPassword))
                return;

            var payload = $"{taiKhoanOrEmail}\n{plainPassword}";
            var bytes = Encoding.UTF8.GetBytes(payload);
            var protectedBytes = MachineKey.Protect(bytes, Purpose);
            var base64 = Convert.ToBase64String(protectedBytes);

            var c = new HttpCookie(CookieName, base64)
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddDays(days),
                Secure = false   
            };
            HttpContext.Current.Response.Cookies.Add(c);
        }

        public static bool Load(out string taiKhoanOrEmail, out string plainPassword)
        {
            taiKhoanOrEmail = null;
            plainPassword = null;

            var c = HttpContext.Current.Request.Cookies[CookieName];
            if (c == null || string.IsNullOrEmpty(c.Value)) return false;

            try
            {
                var protectedBytes = Convert.FromBase64String(c.Value);
                var bytes = MachineKey.Unprotect(protectedBytes, Purpose);
                var s = Encoding.UTF8.GetString(bytes ?? Array.Empty<byte>());

                var parts = s.Split(new[] { '\n' }, 2);
                if (parts.Length == 2)
                {
                    taiKhoanOrEmail = parts[0];
                    plainPassword = parts[1];
                    return true;
                }
            }
            catch { /* ignore */ }

            return false;
        }

        public static void Clear()
        {
            var c = new HttpCookie(CookieName) { Expires = DateTime.Now.AddDays(-1) };
            HttpContext.Current.Response.Cookies.Add(c);
        }
    }
}
