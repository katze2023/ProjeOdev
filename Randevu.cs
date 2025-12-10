using System.ComponentModel.DataAnnotations;

namespace ProjeOdev.Models
{
    public class Randevu
    {
        public int RandevuId { get; set; }

        [Required]
        public DateTime RandevuTarihiSaati { get; set; } // Randevu başlangıç saati [cite: 20]

        public bool OnaylandiMi { get; set; } = false; // Randevu onay mekanizması [cite: 21]

        // İlişkiler
        public int AntrenorId { get; set; }
        public Antrenor Antrenor { get; set; }

        public int HizmetId { get; set; }
        public Hizmet Hizmet { get; set; }

        // Üye (Kullanıcı) Identity entegrasyonundan sonra buraya eklenecek.
        public string UyeId { get; set; } // Identity kullanıcısının ID'si
        // public ApplicationUser Uye { get; set; } // Identity kullanıcısı
    }
}