using FitnessCenterManagement.Models;
using System.Collections.Generic;

namespace FitnessCenterManagement.Services
{
    /// <summary>
    /// Yapay zeka servisinden dönen tüm senaryoları ve durumu paketleyen ana sınıf.
    /// </summary>
    public class GeminiResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }

        // Kullanıcıya sunulacak 3 farklı gelecek senaryosu (Kilo Verme, Kas, Hacim)
        public List<PlanBundle>? PlanBundles { get; set; }

        public static GeminiResult Success(List<PlanBundle> bundles)
        {
            return new GeminiResult
            {
                IsSuccess = true,
                PlanBundles = bundles
            };
        }

        public static GeminiResult Failure(string errorMessage)
        {
            return new GeminiResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }

    /// <summary>
    /// Her bir hedef (Örn: Yağ Yakımı) için diyet, egzersiz ve dönüşüm görselini bir arada tutan paket.
    /// </summary>
    public class PlanBundle
    {
        // Senaryonun adı (Örn: "Hızlı Yağ Yakımı", "Maksimum Kas Kazanımı")
        public string GoalTitle { get; set; } = string.Empty;

        // Hedef tipi (Loss, Gain, Volume)
        public string GoalType { get; set; } = string.Empty;

        // 3 Günlük Egzersiz Listesi
        public List<ExerciseRecommendation> Exercises { get; set; } = new();

        // Senaryoya özel diyet programı
        public string DietPlan { get; set; } = string.Empty;

        // Görsel oluşturma motoru için AI betimlemesi
        public string TransformationDescription { get; set; } = string.Empty;

        // Üretilen "3 ay sonraki haliniz" görselinin Base64 veya URL verisi
        public string? GeneratedImageUrl { get; set; }
    }
}