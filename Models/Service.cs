
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Hizmet adı en fazla 100 karakter olabilir.")]
        [Display(Name = "Hizmet Adı")]
        public string Name { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Süre (dakika) girilmelidir.")]
        [Range(10, 300, ErrorMessage = "Süre 10 ile 300 dakika arasında olmalıdır.")]
        [Display(Name = "Süre (dakika)")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Ücret girilmelidir.")]
        [Range(0, 10000, ErrorMessage = "Ücret 0 ile 10000 arasında olmalıdır.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Ücret (TL)")]
        public decimal Price { get; set; }

        // Eğer birden fazla salon varsa ilişki için:
        public int? SalonId { get; set; }

        [ForeignKey("SalonId")]
        public Salon Salon { get; set; }
    }
}
