using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class ExerciseRecommendation
    {
        public int Id { get; set; }

        [Required]
        public string BodyType { get; set; }

        [Required]
        public string Goal { get; set; } // Kilo Verme, Kas Geliştirme, Fit Kalma



        [Required]
        [StringLength(100)]
        public string Title { get; set; } // Plan başlığı

        [StringLength(500)]
        public string Description { get; set; } // Egzersiz açıklaması
    }
}
