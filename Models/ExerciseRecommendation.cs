using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class ExerciseRecommendation
    {
        public int Id { get; set; }

        [Required]
        public string BodyType { get; set; } // Slim, Athletic, Overweight

        [Required]
        public string Goal { get; set; } // Kilo Verme, Kas Geliştirme, Fit Kalma

        [Required]
        public string ExerciseName { get; set; }

        public string Description { get; set; }

        public int DurationMinutes { get; set; }
    }
}
