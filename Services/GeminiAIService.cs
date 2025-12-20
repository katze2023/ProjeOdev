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
            // Model adı güncel olabilir, kontrol etmekte fayda var.
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

                // Prompt'u JSON üretmeye daha sıkı zorluyoruz
                var prompt = $@"Kullanıcı: {heightCm}cm, {weightKg}kg, {bodyType}, BMI:{bmi:F1}

GÖREV: Bu kullanıcı için 3 FARKLI fitness senaryosu (Kilo Verme, Kas Yapma, Hacim) oluştur.

YANIT FORMATI: Yanıtın SADECE ve SADECE saf JSON olmalı. Markdown (```json) kullanma.

İstenen JSON Yapısı:
{{
  ""bundles"": [
    {{
      ""GoalTitle"": ""Senaryo Başlığı (Örn: Hızlı Yağ Yakımı)"",
      ""GoalType"": ""Loss"" (veya ""Gain"", ""Bulk""),
      ""DietPlan"": ""HTML formatında (<br>, <strong> kullanabilirsin) detaylı beslenme planı"",
      ""Exercises"": [
        {{
          ""Title"": ""1. Gün Program Adı"",
          ""Description"": ""HTML formatında egzersiz listesi (Set x Tekrar)"",
          ""DurationMinutes"": 45,
          ""FocusArea"": ""Tüm Vücut""
        }},
        {{
           ""Title"": ""2. Gün Program Adı"",
           ""Description"": ""..."",
           ""DurationMinutes"": 45,
           ""FocusArea"": ""Kardiyo""
        }},
        {{
           ""Title"": ""3. Gün Program Adı"",
           ""Description"": ""..."",
           ""DurationMinutes"": 45,
           ""FocusArea"": ""Alt Vücut""
        }}
      ]
    }}
  ]
}}";

                var partsList = new List<object> { new { text = prompt } };

                // Görsel varsa ekle (max 500KB - Basit bir kontrol)
                if (!string.IsNullOrEmpty(imageBase64))
                {
                    // Base64 string'in kabaca boyutunu kontrol et
                    if (imageBase64.Length <= 700_000) // Yaklaşık 500KB+ pay
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
                        temperature = 0.4, // Daha tutarlı yanıt için düşürdüm
                        topK = 32,
                        topP = 0.8,
                        maxOutputTokens = 4096,
                        responseMimeType = "application/json" // JSON modunu zorla
                    }
                };

                var requestUrl = $"{_baseUrl}/{_textModel}:generateContent?key={_apiKey}";
                var jsonPayload = JsonSerializer.Serialize(payload);

                var response = await _httpClient.PostAsync(requestUrl,
                    new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    // Hata durumunda loglama veya mock data
                    return await GetMockDataWithBundles($"API Hatası: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);

                if (!doc.RootElement.TryGetProperty("candidates", out var candidates) ||
                    candidates.GetArrayLength() == 0)
                {
                    return await GetMockDataWithBundles("API geçersiz yanıt döndü.");
                }

                // AI'dan gelen ham metin
                var contentText = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrEmpty(contentText))
                {
                    return await GetMockDataWithBundles("API boş yanıt döndü.");
                }

                // -- KRİTİK DÜZELTME: Markdown Temizliği --
                // AI bazen ```json ... ``` şeklinde yanıt verir. Bunu temizlemeliyiz.
                contentText = CleanJsonFromMarkdown(contentText);

                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var aiResponse = JsonSerializer.Deserialize<JsonElement>(contentText);

                // "bundles" array'ini güvenli şekilde al
                if (!aiResponse.TryGetProperty("bundles", out var bundleArrayElement))
                {
                    // Bazen root direkt array olabilir veya yapı farklı olabilir, burada fallback yapabiliriz
                    // Ancak şimdilik prompt'a güvendiğimiz için mock dönüyoruz
                    return await GetMockDataWithBundles("JSON formatı beklenen yapıda değil.");
                }

                var bundleArray = bundleArrayElement.EnumerateArray();
                var bundles = new List<PlanBundle>();

                foreach (var b in bundleArray)
                {
                    var goalType = b.TryGetProperty("GoalType", out var gt) ? gt.GetString() ?? "General" : "General";

                    // Exercises dizisini güvenli parse et
                    var exercises = new List<ExerciseRecommendation>();
                    if (b.TryGetProperty("Exercises", out var exArray))
                    {
                        exercises = JsonSerializer.Deserialize<List<ExerciseRecommendation>>(exArray.ToString(), jsonOptions)
                                    ?? new List<ExerciseRecommendation>();
                    }

                    var bundle = new PlanBundle
                    {
                        GoalTitle = b.TryGetProperty("GoalTitle", out var t) ? t.GetString() ?? "Hedef" : "Hedef",
                        GoalType = goalType,
                        DietPlan = b.TryGetProperty("DietPlan", out var d) ? d.GetString() ?? "" : "",
                        Exercises = exercises
                    };

                    // Unsplash'ten görsel al
                    bundle.GeneratedImageUrl = await GetUnsplashImage(goalType, bodyType);
                    bundles.Add(bundle);
                }

                return GeminiResult.Success(bundles);
            }
            catch (Exception ex)
            {
                // Hata detayını console'a yazmak iyi olur
                Console.WriteLine($"AI Service Error: {ex}");
                return await GetMockDataWithBundles($"Sistem Hatası: {ex.Message}");
            }
        }

        // Markdown temizleyici yardımcı fonksiyon
        private string CleanJsonFromMarkdown(string json)
        {
            if (string.IsNullOrEmpty(json)) return json;

            // ```json veya ``` ile başlıyorsa temizle
            json = json.Trim();
            if (json.StartsWith("```"))
            {
                var firstLineEnd = json.IndexOf('\n');
                if (firstLineEnd > -1)
                {
                    json = json.Substring(firstLineEnd + 1);
                }

                var lastFence = json.LastIndexOf("```");
                if (lastFence > -1)
                {
                    json = json.Substring(0, lastFence);
                }
            }
            return json.Trim();
        }

        private async Task<string> GetUnsplashImage(string goalType, string bodyType)
        {
            // Basitleştirilmiş Unsplash mantığı
            try
            {
                var searchQuery = goalType.ToLower() switch
                {
                    "loss" => "fitness weight loss running",
                    "gain" => "gym muscle bodybuilding",
                    "bulk" => "heavy lifting powerlifting",
                    _ => "gym workout"
                };

                if (string.IsNullOrEmpty(_unsplashAccessKey)) return GetPlaceholderImage(goalType);

                var requestUrl = $"[https://api.unsplash.com/photos/random?query=](https://api.unsplash.com/photos/random?query=){Uri.EscapeDataString(searchQuery)}&orientation=landscape";
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Add("Authorization", $"Client-ID {_unsplashAccessKey}");

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    return doc.RootElement.GetProperty("urls").GetProperty("regular").GetString() ?? GetPlaceholderImage(goalType);
                }
            }
            catch { /* Ignore errors */ }
            return GetPlaceholderImage(goalType);
        }

        private string GetPlaceholderImage(string goalType)
        {
            return goalType.ToLower() switch
            {
                "loss" => "[https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?w=800&q=80](https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?w=800&q=80)",
                "gain" => "[https://images.unsplash.com/photo-1583454110551-21f2fa2afe61?w=800&q=80](https://images.unsplash.com/photo-1583454110551-21f2fa2afe61?w=800&q=80)",
                "bulk" => "[https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=800&q=80](https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=800&q=80)",
                _ => "[https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=800&q=80](https://images.unsplash.com/photo-1517836357463-d25dfeac3438?w=800&q=80)"
            };
        }

        private async Task<GeminiResult> GetMockDataWithBundles(string diagnosticInfo)
        {
            await Task.Delay(100);
            var bundles = new List<PlanBundle>
            {
                new PlanBundle
                {
                    GoalTitle = "Örnek Program (Demo)",
                    GoalType = "Demo",
                    DietPlan = "Bu bir demo verisidir. " + diagnosticInfo,
                    GeneratedImageUrl = GetPlaceholderImage("gain"),
                    Exercises = new List<ExerciseRecommendation>
                    {
                        new ExerciseRecommendation { Title = "1. Gün: Demo Antrenman", Description = "3x12 Bench Press\n3x10 Squat", DurationMinutes = 45, FocusArea = "Full Body" }
                    }
                }
            };
            return GeminiResult.Success(bundles);
        }

        public Task<GeminiResult> GenerateTransformationVisualization(string b, string t) => throw new NotImplementedException();
    }
}