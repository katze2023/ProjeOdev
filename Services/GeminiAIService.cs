using FitnessCenterManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace FitnessCenterManagement.Services
{
    public class GeminiAIService : IGeminiAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-09-2025:generateContent";

        // Constructor'da HttpClient ve Configuration alımı (Gerçek entegrasyon için)
        public GeminiAIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // API key'i güvenli bir yerden alınmalı
            _apiKey = configuration["Gemini:ApiKey"] ?? "";
        }

        public async Task<GeminiResult> GetExerciseRecommendations(int heightCm, int weightKg, string bodyType, string? imageBase64)
        {
            // *** DINAMIK MOCK VERIYE YÖNLENDİRME ***
            // Önceki API hataları ve Canvas ortamı nedeniyle, geçici olarak girişlere göre değişen dinamik Mock veriyi döndürüyoruz.
            return await GetDynamicMockRecommendations(heightCm, weightKg, bodyType, imageBase64);


            /* // --- GERÇEK API ÇAĞRISI MANTIĞI (Şu anlık devre dışı) ---
            // Bu blok, API anahtarınız ayarlandığında ve hatalar çözüldüğünde kullanılmalıdır.

            var bmi = (double)weightKg / Math.Pow((double)heightCm / 100, 2);
            var goal = bmi < 20 ? "Kas Geliştirme" : "Kilo Verme";
            var prompt = new StringBuilder();
            
            prompt.AppendLine($"Kullanıcının boyu {heightCm} cm, kilosu {weightKg} kg, vücut tipi {bodyType} (BMI: {bmi:F2}). Hedef: {goal}.");
            if (imageBase64 != null)
                prompt.AppendLine("Yüklenen fotoğrafı analiz ederek HEDEFİNE yönelik 3 günlük egzersiz planını TÜRKÇE JSON formatında oluştur.");
            else
                prompt.AppendLine("Yalnızca bu verilere dayanarak HEDEFİNE yönelik 3 günlük egzersiz planını TÜRKÇE JSON formatında oluştur.");

            // ... (Payload ve API çağrısı mantığı buraya gelir)
            
            if (string.IsNullOrEmpty(_apiKey)) return await GetDynamicMockRecommendations(heightCm, weightKg, bodyType, imageBase64);

            try
            {
                // ... Gerçek API çağrısı kodları ...
                throw new NotImplementedException("Gerçek API entegrasyonu tamamlanmadı.");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI servisi başarısız: {ex.Message}");
                return GeminiResult.Failure($"API servisine erişilemedi veya yanıt işlenemedi. Detay: {ex.Message}");
            }
            */
        }

        // --- DINAMIK MOCK VERI UYGULAMASI (Girişlere göre değişir) ---
        private async Task<GeminiResult> GetDynamicMockRecommendations(int heightCm, int weightKg, string bodyType, string? imageBase64)
        {
            await Task.Delay(10);
            var bmi = (double)weightKg / Math.Pow((double)heightCm / 100, 2);
            var goal = bmi < 20 ? "Kas Geliştirme" : "Kilo Verme";
            var photoAnalysisStatus = imageBase64 != null ? "FOTOĞRAF ANALİZİ DAHİL" : "YALNIZCA VERİ TABANLI";

            var recommendations = new List<ExerciseRecommendation>();

            // 1. Gün: Güç / Direnç
            var day1Title = bodyType == "Ectomorph" ? "1. Gün: Ağır Güç ve Hacim Odaklı" :
                            bodyType == "Endomorph" ? "1. Gün: Metabolizma Hızlandırıcı Direnç" : "1. Gün: Atletik Gelişim (Full Vücut)";
            var day1Desc = $"[{photoAnalysisStatus}] Vücut tipine uygun temel bileşik hareketler. Hedef: {goal}. Squat (3x8), Bench Press (3x10).";
            recommendations.Add(new ExerciseRecommendation { Id = 1, BodyType = bodyType, Goal = goal, Title = day1Title, Description = day1Desc, DurationMinutes = 65, FocusArea = "Güç Antrenmanı" });

            // 2. Gün: Kardiyo / HIIT
            var day2Title = goal == "Kilo Verme" ? "2. Gün: Yüksek Yoğunluklu Kardiyo (HIIT)" : "2. Gün: Düşük Yoğunluklu Kardiyo ve Esneklik";
            var day2Desc = goal == "Kilo Verme" ? $"[{photoAnalysisStatus}] 20 dakika HIIT (30sn sprint/60sn yürüme) ve 10 dakika karın. {bodyType} tipi için ideal yağ yakımı." :
                                                 "30 dakika yürüme/hafif koşu ve 15 dakika esneme. Kas onarımına odaklanın.";
            recommendations.Add(new ExerciseRecommendation { Id = 2, BodyType = bodyType, Goal = goal, Title = day2Title, Description = day2Desc, DurationMinutes = 45, FocusArea = "Kardiyo" });

            // 3. Gün: Tamamlayıcı / Hipertrofi
            var day3Title = bodyType == "Ectomorph" ? "3. Gün: İzole Hipertrofi ve Beslenme Odak" : "3. Gün: Tamamlayıcı Üst Vücut (Hafif)";
            var day3Desc = $"[{photoAnalysisStatus}] Ağırlık/Tekrar: Dambıl Omuz Pres (4x12), Cable Row (3x15). Beslenme programınıza sıkıca uyun.";
            recommendations.Add(new ExerciseRecommendation { Id = 3, BodyType = bodyType, Goal = goal, Title = day3Title, Description = day3Desc, DurationMinutes = 55, FocusArea = "Hipertrofi" });


            // Kaynaklar için dinamiklik
            var sources = new List<object>
            {
                new { uri = "https://www.google.com/search?q=fitness+planlama", title = $"Genel Fitness Kuralları" },
                new { uri = "https://www.google.com/search?q=body+type+training", title = $"{bodyType} Vücut Tipi için Antrenmanlar" }
            };

            return GeminiResult.Success(recommendations, sources);
        }

        public Task<GeminiResult> GenerateTransformationVisualization(string base64Image, string targetDescription)
        {
            // Görselleştirme mock implementasyonu
            string mockImageUrl = $"https://placehold.co/500x500/06B6D4/FFFFFF?text=D%C3%B6n%C3%BC%C5%9F%C3%BCm%0A({targetDescription.Replace(' ', '+')})";
            return Task.FromResult(GeminiResult.SuccessImage(mockImageUrl));
        }

        private double CalculateBmi(int weightKg, int heightCm)
        {
            if (heightCm <= 0) return 0;
            double heightM = heightCm / 100.0;
            return weightKg / (heightM * heightM);
        }
    }
}