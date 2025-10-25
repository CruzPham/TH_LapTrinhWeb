using System;
using System.ComponentModel.DataAnnotations;

namespace PhamVanTung.SachOnline.Models.ViewModels
{
    public class DangKyVM
    {
        [Required(ErrorMessage = "Vui long nhap ho ten")]
        [StringLength(50, ErrorMessage = "Ho ten toi da 50 ky tu")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui long nhap ten dang nhap")]
        [StringLength(15, MinimumLength = 3, ErrorMessage = "Ten dang nhap 3-15 ky tu")]
        public string TaiKhoan { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 6, ErrorMessage = "Mat khau 6-15 ky tu")]
        public string MatKhau { get; set; }

        [Compare("MatKhau", ErrorMessage = "Mat khau nhap lai khong khop")]
        public string NhapLaiMatKhau { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email khong hop le")]
        [StringLength(50, ErrorMessage = "Email toi da 50 ky tu")]
        public string Email { get; set; }

        [StringLength(10, ErrorMessage = "Dien thoai toi da 10 ky tu")]
        public string DienThoai { get; set; }
        public DateTime? NgaySinh { get; set; }

        [StringLength(100, ErrorMessage = "Dia chi toi da 100 ky tu")]
        public string DiaChi { get; set; }
    }
}
