using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class ExerciseRecommendation
    {
        public int Id { get; set; }

        [Required]
        // Hangi vücut tipine yönelik olduğu
        public string BodyType { get; set; }

        [Required]
        // Hangi hedefe yönelik olduğu (Kilo Verme, Kas Geliştirme, vb.)
        public string Goal { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } // Plan başlığı

        [StringLength(500)]
        public string Description { get; set; } // Egzersiz açıklaması

        // Önceki versiyondan kalan faydalı alanları koruyorum
        public int DurationMinutes { get; set; }
        public string FocusArea { get; set; }
    }
}