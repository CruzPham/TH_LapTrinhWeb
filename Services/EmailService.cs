using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using PhamVanTung.SachOnline.Models;

namespace PhamVanTung.SachOnline.Services
{
    public static class EmailService
    {
        static readonly CultureInfo vi = new CultureInfo("vi-VN");

        // Gmail SMTP (App Password 16 ký tự)
        private static readonly string fromEmail = "phamvantung.cr25@gmail.com";
        private static readonly string smtpUser = "phamvantung.cr25@gmail.com";
        private static readonly string smtpPass = "ivupewrspnmlkgds"; // <— app password 

        public static void SendOrderConfirmation(KHACHHANG kh, DONDATHANG dh, IEnumerable<GIOHANG> items)
        {
            // 1) Validate đầu vào
            if (kh == null) throw new ArgumentNullException(nameof(kh));
            if (string.IsNullOrWhiteSpace(kh.Email))
                throw new InvalidOperationException("Email khách hàng trống — hãy điền KHACHHANG.Email trước khi gửi.");

            // 2) Tạo mail
            var msg = new MailMessage
            {
                From = new MailAddress(fromEmail, "Sách Online - Xác nhận đơn hàng", Encoding.UTF8),
                Subject = "Xác nhận đơn hàng",
                SubjectEncoding = Encoding.UTF8,
                Body = BuildBody(kh, dh, items),
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };
            msg.To.Add(new MailAddress(kh.Email, kh.HoTen ?? kh.TaiKhoan));

            // 3) Ép TLS 1.2 (phòng .NET/OS cũ)
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            // 4) Thử 587 (STARTTLS) -> nếu lỗi, fallback 465 (SSL)
            try
            {
                SendCore(msg, host: "smtp.gmail.com", port: 587, enableSsl: true);
                System.Diagnostics.Debug.WriteLine($"✅ Đã gửi xác nhận tới {kh.Email} (587/TLS).");
            }
            catch (Exception ex587)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ 587/TLS lỗi: " + ex587.ToString());
                // Fallback 465
                SendCore(msg, host: "smtp.gmail.com", port: 465, enableSsl: true);
                System.Diagnostics.Debug.WriteLine($"✅ Đã gửi xác nhận tới {kh.Email} (465/SSL).");
            }
        }

        private static void SendCore(MailMessage msg, string host, int port, bool enableSsl)
        {
            using (var smtp = new SmtpClient(host, port))
            {
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtp.EnableSsl = enableSsl;
                smtp.Timeout = 30000; // 30s
                smtp.Send(msg);
            }
        }

        private static string BuildBody(KHACHHANG kh, DONDATHANG dh, IEnumerable<GIOHANG> items)
        {
            var sb = new StringBuilder();
            sb.Append(@"
<div style='font-family:Arial,Helvetica,sans-serif;max-width:720px;margin:auto;color:#222'>
  <h2 style='font-size:22px;margin-bottom:5px;'>Xác nhận đơn hàng</h2>
  <p>Chúng tôi vừa nhận đơn hàng của <b>" + (kh.HoTen ?? kh.TaiKhoan) + @"</b>.</p>
  <p>Đơn hàng của bạn đã được đặt thành công.</p>

  <h3 style='margin-top:24px'>Chi tiết đơn hàng:</h3>
  <table role='presentation' cellpadding='8' cellspacing='0' width='100%' 
         style='border-collapse:collapse;border:1px solid #e5e7eb'>
    <thead>
      <tr style='background:#f3f4f6;text-align:left'>
        <th style='width:60px;border-bottom:1px solid #e5e7eb'>STT</th>
        <th style='border-bottom:1px solid #e5e7eb'>Tên sản phẩm</th>
        <th style='width:120px;border-bottom:1px solid #e5e7eb;text-align:center'>Số lượng</th>
        <th style='width:140px;border-bottom:1px solid #e5e7eb;text-align:right'>Đơn giá</th>
      </tr>
    </thead>
    <tbody>");
            int stt = 1; decimal tong = 0;
            foreach (var it in items)
            {
                var donGia = (decimal)it.dDonGia;
                tong += donGia * it.iSoLuong;
                sb.Append($@"
      <tr style='border-top:1px solid #f1f5f9'>
        <td>{stt++}</td>
        <td>{System.Web.HttpUtility.HtmlEncode(it.sTenSach)}</td>
        <td style='text-align:center'>{it.iSoLuong}</td>
        <td style='text-align:right'>{donGia.ToString("#,0", vi)} VNĐ</td>
      </tr>");
            }
            sb.Append(@"
    </tbody>
    <tfoot>
      <tr>
        <td colspan='3' style='text-align:right;font-weight:bold;border-top:1px solid #e5e7eb'>Tổng tiền:</td>
        <td style='text-align:right;font-weight:bold;border-top:1px solid #e5e7eb'>" +
        tong.ToString("#,0", vi) + @" VNĐ</td>
      </tr>
    </tfoot>
  </table>

  <p style='margin-top:18px'>
    Ngày đặt: " + dh.NgayDat?.ToString("dd/MM/yyyy") + @"<br/>
    Ngày giao dự kiến: " + dh.NgayGiao?.ToString("dd/MM/yyyy") + @"<br/>
    Mã đơn hàng: <b>" + dh.MaDonHang + @"</b>
  </p>

  <p style='margin-top:24px'>Cảm ơn bạn đã mua hàng tại cửa hàng chúng tôi!</p>
</div>");
            return sb.ToString();
        }
    }
}
