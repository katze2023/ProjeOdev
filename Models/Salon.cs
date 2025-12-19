using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Models
{
    public class Salon
    {

        public int Id { get; set; }
        public string Ad { get; set; }    
        public string Adres { get; set; }
        public string CalismaSaatleri { get; set; }


        // Salon resmi için
        [Display(Name = "Salon Görseli")]
        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public ICollection<Trainer>? Trainers { get; set; }

    }
}
