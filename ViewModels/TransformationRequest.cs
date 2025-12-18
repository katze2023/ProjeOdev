using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FitnessCenterManagement.Models.ViewModels
{
    // Vücut dönüşümü görselleştirme isteği için gerekli verileri tutar
    public class TransformationRequest
    {
        [Required(ErrorMessage = "Lütfen güncel fotoğrafınızı yükleyin.")]
        [Display(Name = "Mevcut Vücut Fotoğrafı")]
        public IFormFile ImageFile { get; set; }

        [Required(ErrorMessage = "Hedefinizi kısaca açıklayın.")]
        [StringLength(200, ErrorMessage = "Hedef tanımı en fazla 200 karakter olmalıdır.")]
        [Display(Name = "Hedef Görünüm Tanımı")]
        public string TargetDescription { get; set; }
    }
}