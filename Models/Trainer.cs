using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Antrenör adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Uzmanlık alanı belirtilmelidir.")]
        [StringLength(250)]
        [Display(Name = "Uzmanlık Alanları")]
        public string Expertise { get; set; }

        [StringLength(250)]
        [Display(Name = "Müsaitlik Saatleri")]
        public string AvailableHours { get; set; }

        [Display(Name = "Salon")]
        public int? SalonId { get; set; }

        [ForeignKey("SalonId")]
        public Salon Salon { get; set; }

        // Antrenör resmi
        public string? ImagePath { get; set; }

    }
}
