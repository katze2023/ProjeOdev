using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models.ViewModels
{
    public class AIUserInputViewModel
    {
        [Required]
        [Display(Name = "Boy (cm)")]
        public int HeightCm { get; set; }

        [Required]
        [Display(Name = "Kilo (kg)")]
        public int WeightKg { get; set; }

        [Required]
        [Display(Name = "Vücut Tipi")]
        public string BodyType { get; set; }
    }
}
