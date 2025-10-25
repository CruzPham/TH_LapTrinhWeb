using System.ComponentModel.DataAnnotations;

namespace PhamVanTung.SachOnline.Models.ViewModels
{
    public class DangNhapVM
    {
        [Required(ErrorMessage = "Vui long nhap tai khoan hoac email")]
        [Display(Name = "Tai khoan / Email")]
        public string TaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui long nhap mat khau")]
        [DataType(DataType.Password)]
        [Display(Name = "Mat khau")]
        public string MatKhau { get; set; }

        [Display(Name = "Nho dang nhap")]
        public bool RememberMe { get; set; }

        [ScaffoldColumn(false)]
        public string ReturnUrl { get; set; }
    }
}
