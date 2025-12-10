using System.ComponentModel.DataAnnotations;

namespace ProjeOdev.Models
{
    public class Hizmet
    {
        public int HizmetId { get; set; }

        [Required(ErrorMessage = "Hizmet Adı zorunludur.")]
        [StringLength(100)]
        public string Ad { get; set; }

        [StringLength(500)]
        public string Aciklama { get; set; }

        [Required(ErrorMessage = "Süre zorunludur.")]
        public int SureDakika { get; set; } // Dakika cinsinden süre [cite: 12]

        [Required(ErrorMessage = "Ücret zorunludur.")]
        public decimal Ucret { get; set; } // Ücret [cite: 12]

        // Bu hizmeti verebilen antrenörlerin listesi (Many-to-Many ilişkisi için)
        public ICollection<AntrenorHizmet> AntrenorHizmetleri { get; set; }
    }
}
