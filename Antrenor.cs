using System.ComponentModel.DataAnnotations;

namespace ProjeOdev.Models
{
    public class Antrenor
    {
        public int AntrenorId { get; set; }

        [Required]
        [StringLength(100)]
        public string AdSoyad { get; set; }

        [StringLength(255)]
        public string UzmanlikAlanlari { get; set; } // Örneğin: Kas Geliştirme, Yoga, Kilo Verme [cite: 16]

        public bool MusaitMi { get; set; } = true;

        // Antrenörün verebildiği hizmetler (Many-to-Many ilişkisi için)
        public ICollection<AntrenorHizmet> AntrenorHizmetleri { get; set; }

        // Antrenörün randevuları (One-to-Many ilişkisi)
        public ICollection<Randevu> Randevular { get; set; }

        // Antrenörün müsaitlik saatleri için ek bir tabloya ihtiyaç duyulabilir, 
        // ancak basit başlangıç için şimdilik burada tutalım.
        public string MusaitlikSaatleriJson { get; set; } // Örn: Pazartesi: 09:00-18:00 (Bu daha sonra detaylandırılacak) [cite: 17]
    }
}