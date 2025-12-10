namespace ProjeOdev.Models
{
    // Antrenörlerin hangi hizmetleri verebildiğini tutan bağlayıcı tablo
    public class AntrenorHizmet
    {
        public int AntrenorId { get; set; }
        public Antrenor Antrenor { get; set; }

        public int HizmetId { get; set; }
        public Hizmet Hizmet { get; set; }
    }
}
