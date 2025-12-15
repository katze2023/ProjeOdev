using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Boy (cm)")]
        public int? HeightCm { get; set; }

        [Display(Name = "Kilo (kg)")]
        public int? WeightKg { get; set; }

        [Display(Name = "Vücut Tipi")]
        [StringLength(50)]
        public string? BodyType { get; set; }

        //[Display(Name = "Profil Fotoğrafı")]
        //public string? ProfileImagePath { get; set; }
    }
}
