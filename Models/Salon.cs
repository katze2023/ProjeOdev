namespace FitnessCenterManagement.Models
{
    public class Salon
    {

        public int Id { get; set; }
        public string Ad { get; set; }    // "Ad" alanı burada tanımlı olmalı
        public string Adres { get; set; }
        public string CalismaSaatleri { get; set; }

        // Salon resmi
       // public string? ImagePath { get; set; }

        public ICollection<Trainer>? Trainers { get; set; }

    }
}
