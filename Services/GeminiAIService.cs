using FitnessCenterManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading;

namespace FitnessCenterManagement.Services
{
    public class GeminiAIService : IGeminiAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _textModel;
        private readonly string _unsplashAccessKey;

        // Rate Limiting
        private static readonly SemaphoreSlim _rateLimiter = new SemaphoreSlim(1, 1);
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private const int MinMillisecondsBetweenRequests = 4000;

        public GeminiAIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiAPI:ApiKey"] ?? "";
            _unsplashAccessKey = configuration["Unsplash:AccessKey"] ?? "";
            _baseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
            _textModel = "gemini-2.0-flash-exp";
        }

        public async Task<GeminiResult> GetExerciseRecommendations(int heightCm, int weightKg, string bodyType, string? imageBase64)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return await GetMockDataWithBundles("API anahtarı bulunamadı.");

            // Rate Limiting
            await _rateLimiter.WaitAsync();
            try
            {
                var timeSinceLastRequest = (DateTime.Now - _lastRequestTime).TotalMilliseconds;
                if (timeSinceLastRequest < MinMillisecondsBetweenRequests)
                {
                    var delayMs = (int)(MinMillisecondsBetweenRequests - timeSinceLastRequest);
                    await Task.Delay(delayMs);
                }
                _lastRequestTime = DateTime.Now;
            }
            finally
            {
                _rateLimiter.Release();
            }

            try
            {
                var bmi = weightKg / Math.Pow(heightCm / 100.0, 2);

                var prompt = $@"Kullanıcı: {heightCm}cm, {weightKg}kg, {bodyType}, BMI:{bmi:F1}

3 FARKLI senaryo oluştur:
1. ""Kilo Verme & Yağ Yakımı"" (GoalType: ""Loss"")
2. ""Atletik Kas İnşası"" (GoalType: ""Gain"")  
3. ""Hacim Kazanma (Bulk)"" (GoalType: ""Bulk"")

Her senaryo için TÜRKÇE:
- GoalTitle: Çekici başlık
- GoalType: Loss/Gain/Bulk
- DietPlan: Detaylı günlük beslenme planı (kahvaltı, öğle, akşam, ara öğün)
- Exercises: 3 günlük egzersiz programı

Exercises formatı:
- Title: ""1. Gün: Program Adı""
- Description: Detaylı egzersiz açıklaması (setler, tekrarlar)
- DurationMinutes: Süre
- FocusArea: Odak alanı

SADECE JSON formatında yanıt ver:
{{
  ""bundles"": [
    {{
      ""GoalTitle"": ""Senaryo Başlığı"",
      ""GoalType"": ""Loss"",
      ""DietPlan"": ""Detaylı diyet planı..."",
      ""Exercises"": [
        {{
          ""Title"": ""1. Gün: Tam Vücut"",
          ""Description"": ""Egzersiz detayları..."",
          ""DurationMinutes"": 60,
          ""FocusArea"": ""Kas Geliştirme""
        }}
      ]
    }}
  ]
}}";

                var partsList = new List<object> { new { text = prompt } };

                // Görsel varsa ekle (max 500KB)
                if (!string.IsNullOrEmpty(imageBase64))
                {
                    var imageBytes = Convert.FromBase64String(imageBase64);
                    if (imageBytes.Length <= 500_000)
                    {
                        partsList.Add(new
                        {
                            inline_data = new
                            {
                                mime_type = "image/jpeg",
                                data = imageBase64
                            }
                        });
                    }
                }

                var payload = new
                {
                    contents = new[] { new { parts = partsList.ToArray() } },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 4096,
                        responseMimeType = "application/json"
                    }
                };

                var requestUrl = $"{_baseUrl}/{_textModel}:generateContent?key={_apiKey}";
                var jsonPayload = JsonSerializer.Serialize(payload);

                Console.WriteLine($"📤 Gemini API'ye istek gönderiliyor...");

                var response = await _httpClient.PostAsync(requestUrl,
                    new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ API Hatası: {response.StatusCode}");

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        return await GetMockDataWithBundles(
                            "⚠️ API kota limiti aşıldı!\n\n" +
                            "Çözümler:\n" +
                            "1. Yeni API anahtarı: https://aistudio.google.com/app/apikey\n" +
                            "2. 1 saat bekleyin ve tekrar deneyin\n" +
                            "3. Google Cloud ücretli planına geçin"
                        );
                    }

                    return await GetMockDataWithBundles($"API Hatası: {response.StatusCode}");
                }

                using var doc = JsonDocument.Parse(responseContent);

                if (!doc.RootElement.TryGetProperty("candidates", out var candidates) ||
                    candidates.GetArrayLength() == 0)
                {
                    return await GetMockDataWithBundles("API geçersiz yanıt döndü.");
                }

                var contentText = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrEmpty(contentText))
                {
                    return await GetMockDataWithBundles("API boş yanıt döndü.");
                }

                Console.WriteLine($"✅ AI yanıtı alındı");

                var aiResponse = JsonSerializer.Deserialize<JsonElement>(contentText);
                var bundleArray = aiResponse.GetProperty("bundles").EnumerateArray();

                var bundles = new List<PlanBundle>();
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                foreach (var b in bundleArray)
                {
                    var goalType = b.TryGetProperty("GoalType", out var gt) ? gt.GetString() ?? "General" : "General";

                    var bundle = new PlanBundle
                    {
                        GoalTitle = b.GetProperty("GoalTitle").GetString() ?? "Hedef",
                        GoalType = goalType,
                        DietPlan = b.GetProperty("DietPlan").GetString() ?? "Diyet planı belirtilmedi.",
                        Exercises = JsonSerializer.Deserialize<List<ExerciseRecommendation>>(
                            b.GetProperty("Exercises").ToString(), jsonOptions) ?? new List<ExerciseRecommendation>()
                    };

                    // Unsplash'ten hedef tipine göre görsel al
                    bundle.GeneratedImageUrl = await GetUnsplashImage(goalType, bodyType);
                    bundles.Add(bundle);
                }

                Console.WriteLine($"✅ {bundles.Count} senaryo oluşturuldu!");
                return GeminiResult.Success(bundles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                return await GetMockDataWithBundles($"Sistem Hatası: {ex.Message}");
            }
        }

        private async Task<string> GetUnsplashImage(string goalType, string bodyType)
        {
            try
            {
                // Hedef tipine göre arama terimleri
                var searchQuery = goalType.ToLower() switch
                {
                    "loss" => "weight loss fitness cardio running gym",
                    "gain" => "bodybuilding muscle athlete strength training",
                    "bulk" => "powerlifting muscular bodybuilder heavy weights",
                    _ => "fitness workout gym motivation"
                };

                if (string.IsNullOrEmpty(_unsplashAccessKey))
                {
                    Console.WriteLine("⚠️ Unsplash API anahtarı yok, placeholder kullanılıyor");
                    return GetPlaceholderImage(goalType);
                }

                var requestUrl = $"https://api.unsplash.com/photos/random?query={Uri.EscapeDataString(searchQuery)}&orientation=landscape&content_filter=high";

                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Add("Authorization", $"Client-ID {_unsplashAccessKey}");
                request.Headers.Add("Accept-Version", "v1");

                Console.WriteLine($"📸 Unsplash'ten görsel alınıyor: {searchQuery}");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    var imageUrl = doc.RootElement
                        .GetProperty("urls")
                        .GetProperty("regular")
                        .GetString();

                    Console.WriteLine($"✅ Unsplash görsel alındı: {goalType}");
                    return imageUrl ?? GetPlaceholderImage(goalType);
                }
                else
                {
                    Console.WriteLine($"⚠️ Unsplash hatası: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Unsplash exception: {ex.Message}");
            }

            return GetPlaceholderImage(goalType);
        }

        private string GetPlaceholderImage(string goalType)
        {
            return goalType.ToLower() switch
            {
                "loss" => "https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?w=800&q=80", // Cardio/Running
                "gain" => "https://images.unsplash.com/photo-1583454110551-21f2fa2afe61?w=800&q=80", // Bodybuilding
                "bulk" => "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=800&q=80", // Powerlifting
                _ => "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=800&q=80" // Generic gym
            };
        }

        private async Task<GeminiResult> GetMockDataWithBundles(string diagnosticInfo)
        {
            await Task.Delay(100);

            var bundles = new List<PlanBundle>
            {
                new PlanBundle
                {
                    GoalTitle = "🔴 Servis Geçici Olarak Kullanılamıyor",
                    GoalType = "Error",
                    DietPlan = diagnosticInfo,
                    GeneratedImageUrl = "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=800&q=80",
                    Exercises = new List<ExerciseRecommendation>
                    {
                        new ExerciseRecommendation
                        {
                            Title = "⚠️ API Bağlantı Sorunu",
                            Description = "Lütfen API anahtarınızı kontrol edin veya birkaç dakika sonra tekrar deneyin.",
                            DurationMinutes = 0,
                            FocusArea = "Sistem Mesajı"
                        }
                    }
                }
            };

            return GeminiResult.Success(bundles);
        }

        public Task<GeminiResult> GenerateTransformationVisualization(string b, string t) =>
            throw new NotImplementedException();
    }
}