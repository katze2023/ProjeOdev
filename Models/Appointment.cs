using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }  // Identity User FK

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        [Display(Name = "Antrenör")]
        public int TrainerId { get; set; }

        [ForeignKey("TrainerId")]
        public Trainer Trainer { get; set; }

        [Required]
        [Display(Name = "Hizmet")]
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public Service Service { get; set; }

        [Required]
        [Display(Name = "Tarih")]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Saat")]
        public TimeSpan Time { get; set; }

        [Display(Name = "Onay Durumu")]
        public bool IsApproved { get; set; } = false;
    }
}
