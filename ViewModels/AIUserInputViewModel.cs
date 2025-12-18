 using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // IFormFile için gerekli

namespace FitnessCenterManagement.Models.ViewModels
{
    public class AIUserInputViewModel
    {
        [Required(ErrorMessage = "Boy bilgisi zorunludur.")]
        [Display(Name = "Boy (cm)")]
        [Range(100, 250, ErrorMessage = "Boy 100 ile 250 cm arasında olmalıdır.")]
        public int HeightCm { get; set; }

        [Required(ErrorMessage = "Kilo bilgisi zorunludur.")]
        [Display(Name = "Kilo (kg)")]
        [Range(30, 300, ErrorMessage = "Kilo 30 ile 300 kg arasında olmalıdır.")]
        public int WeightKg { get; set; }

        [Required(ErrorMessage = "Vücut Tipi zorunludur.")]
        [Display(Name = "Vücut Tipi")]
        public string BodyType { get; set; }

        [Display(Name = "Vücut Fotoğrafı (isteğe bağlı)")]
        [DataType(DataType.Upload)]
        // IFormFile, kullanıcının yüklediği dosyayı temsil eder.
        public IFormFile? ImageFile { get; set; }
    }
}