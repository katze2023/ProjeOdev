using FitnessCenterManagement.Models;
using Microsoft.Extensions.Configuration; // API Anahtarını almak için
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace FitnessCenterManagement.Services
{
    // *** Sunucu Taraflı Gemini Entegrasyonu ***
    public class AIExerciseService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-09-2025:generateContent";

        public AIExerciseService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // API Anahtarınızı configuration (appsettings.json veya user secrets) dosyasından almalısınız.
            _apiKey = configuration["Gemini:ApiKey"];
            // _httpClient.DefaultRequestHeaders.Authorization = ... (Gerekiyorsa)
        }

        public async Task<IEnumerable<ExerciseRecommendation>> GetRecommendations(ApplicationUser user, string? imageBase64 = null)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                // API anahtarı eksikse mock veri döndür
                return await GetMockRecommendations(user);
            }

            // 1. Prompt Oluşturma
            var bmi = CalculateBmi(user.WeightKg ?? 0, user.HeightCm ?? 0);
            var bodyType = user.BodyType ?? "Genel";
            var goal = bmi < 20 ? "Kas Geliştirme" : "Kilo Verme";

            var prompt = new StringBuilder();
            prompt.AppendLine($"Kullanıcının boyu {user.HeightCm} cm, kilosu {user.WeightKg} kg, vücut tipi {bodyType} (BMI: {bmi:F2}). Hedef: {goal}.");

            if (imageBase64 != null)
            {
                prompt.AppendLine("Yüklenen vücut fotoğrafını ve bu verileri analiz ederek, hedefine yönelik 3 günlük, zorlayıcı bir egzersiz planını TÜRKÇE olarak JSON formatında oluştur.");
            }
            else
            {
                prompt.AppendLine("Yalnızca bu verilere dayanarak, hedefine yönelik 3 günlük, zorlayıcı bir egzersiz planını TÜRKÇE olarak JSON formatında oluştur.");
            }

            // 2. JSON Payload (Yük) Oluşturma
            // Bu kısım, JavaScript'teki karmaşık JSON yapısını yansıtır.
            var parts = new List<object> { new { text = prompt.ToString() } };

            if (imageBase64 != null)
            {
                parts.Add(new
                {
                    inlineData = new
                    {
                        mimeType = "image/jpeg", // Varsayılan JPEG
                        data = imageBase64
                    }
                });
            }

            // Response için beklenen JSON yapısını tanımlayan C# sınıflarını JSON olarak gönderemeyeceğimiz için
            // Yapılandırılmış JSON yanıtı için ResponseSchema'yı doğrudan JSON string olarak gömmemiz gerekir.
            var payload = new
            {
                contents = new[]
                {
                    new { role = "user", parts = parts.ToArray() }
                },
                tools = new[] { new { google_search = new { } } },
                systemInstruction = new { parts = new[] { new { text = "Sen sertifikalı bir fitness uzmanısın. Tüm yanıtlarını daima Türkçe ve sadece JSON formatında üret. ResponseSchema yapısını kesinlikle takip et." } } },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    responseSchema = new
                    {
                        type = "ARRAY",
                        items = new
                        {
                            type = "OBJECT",
                            properties = new
                            {
                                Title = new { type = "STRING", description = "Plan başlığı" },
                                Description = new { type = "STRING", description = "Egzersiz detayları" },
                                DurationMinutes = new { type = "NUMBER", description = "Süre (dk)" },
                                FocusArea = new { type = "STRING", description = "Odak alanı" },
                                BodyType = new { type = "STRING", description = bodyType }, // Model için ipucu
                                Goal = new { type = "STRING", description = goal } // Model için ipucu
                            }
                        }
                    }
                }
            };

            var payloadJson = JsonSerializer.Serialize(payload);
            var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            // 3. API Çağrısı
            // API Anahtarını URL'e ekleyin
            var requestUrl = $"{GeminiApiUrl}?key={_apiKey}";
            var response = await _httpClient.PostAsync(requestUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();

                // Gemini API'dan gelen yanıtı işleme
                using (JsonDocument doc = JsonDocument.Parse(responseJson))
                {
                    var resultCandidate = doc.RootElement
                                             .GetProperty("candidates")[0]
                                             .GetProperty("content")
                                             .GetProperty("parts")[0]
                                             .GetProperty("text").GetString();

                    // Modelin ürettiği saf JSON string'ini C# listesine çevirme
                    var recommendations = JsonSerializer.Deserialize<List<ExerciseRecommendation>>(
                        resultCandidate,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    // BodyType ve Goal alanlarını ekleyin (Gemini bunları string içinde ürettiği için)
                    recommendations.ForEach(r => { r.BodyType = bodyType; r.Goal = goal; });
                    return recommendations ?? Enumerable.Empty<ExerciseRecommendation>();
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Gemini API Hatası: {response.StatusCode} - {errorContent}");
                // Hata durumunda mock veri dönme
                return await GetMockRecommendations(user);
            }
        }

        // Mock veri döndürme metodu (API başarısız olursa veya anahtar yoksa kullanılır)
        private async Task<IEnumerable<ExerciseRecommendation>> GetMockRecommendations(ApplicationUser user)
        {
            await Task.Delay(500);
            var bodyType = user.BodyType ?? "Genel";
            var bmi = CalculateBmi(user.WeightKg ?? 0, user.HeightCm ?? 0);
            var goal = bmi < 20 ? "Kas Geliştirme" : "Kilo Verme";

            return new List<ExerciseRecommendation>
            {
                new ExerciseRecommendation { Id=1, BodyType = bodyType, Goal = goal, Title = "1. Gün: Full Vücut Güç", Description = "Mock Veri: Bench Press ve Squat.", DurationMinutes = 60, FocusArea = "Güç Antrenmanı" },
                new ExerciseRecommendation { Id=2, BodyType = bodyType, Goal = goal, Title = "2. Gün: Kardiyo", Description = "Mock Veri: 30 dakika koşu.", DurationMinutes = 45, FocusArea = "Kardiyo" }
            };
        }

        private double CalculateBmi(int weightKg, int heightCm)
        {
            if (heightCm <= 0) return 0;
            double heightM = heightCm / 100.0;
            return weightKg / (heightM * heightM);
        }
    }
}