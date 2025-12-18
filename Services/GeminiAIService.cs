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
        private const string TextModelUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-09-2025:generateContent";
        private const string ImageModelUrl = "https://generativelanguage.googleapis.com/v1beta/models/imagen-4.0-generate-001:predict";

        public GeminiAIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? "";
        }

        public async Task<GeminiResult> GetExerciseRecommendations(int heightCm, int weightKg, string bodyType, string? imageBase64)
        {
            if (string.IsNullOrEmpty(_apiKey)) return await GetMockData(heightCm, weightKg, bodyType);

            try
            {
                var bmi = (double)weightKg / Math.Pow((double)heightCm / 100, 2);
                var goal = bmi < 20 ? "Kilo Alma ve Kas Kütlesi" : "Yağ Yakımı ve Sıkılaşma";

                // AI'dan ne istediğimizi açıkça belirttiğimiz prompt
                var prompt = $@"
                    Kullanıcı Bilgileri: Boy {heightCm}cm, Kilo {weightKg}kg, Vücut Tipi {bodyType}. Hedef: {goal}.
                    Görev: 
                    1. Bu kullanıcıya özel 3 günlük profesyonel bir egzersiz programı hazırla.
                    2. Bu kullanıcıya özel günlük alması gereken kalori/makro değerlerini ve örnek bir diyet menüsü hazırla.
                    3. Bu kullanıcı eğer önerdiğin planı 3 ay boyunca uygularsa fiziksel görünümünde ne gibi değişimler olacağını detaylıca açıkla.
                    
                    Tüm yanıtı TÜRKÇE ve aşağıdaki JSON formatında ver:
                    {{
                        ""recommendations"": [{{ ""Title"": """", ""Description"": """", ""DurationMinutes"": 0, ""FocusArea"": """" }}],
                        ""dietPlan"": ""Kalori değerleri ve öğün listesi buraya..."",
                        ""transformationPrediction"": ""3 ay sonraki fiziksel değişim açıklaması buraya...""
                    }}";

                var payload = new
                {
                    contents = new[] { new { parts = new[] { new { text = prompt } } } },
                    generationConfig = new { responseMimeType = "application/json" }
                };

                var response = await _httpClient.PostAsync($"{TextModelUrl}?key={_apiKey}",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode) throw new Exception("AI Servisi yanıt vermedi.");

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);
                var content = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                var resultData = JsonSerializer.Deserialize<JsonElement>(content!);

                var recs = JsonSerializer.Deserialize<List<ExerciseRecommendation>>(resultData.GetProperty("recommendations").ToString());
                var diet = resultData.GetProperty("dietPlan").GetString();
                var prediction = resultData.GetProperty("transformationPrediction").GetString();

                return GeminiResult.Success(recs!, diet!, prediction!);
            }
            catch (Exception ex)
            {
                return GeminiResult.Failure($"Hata: {ex.Message}");
            }
        }

        public async Task<GeminiResult> GenerateTransformationVisualization(string base64Image, string targetDescription)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return GeminiResult.SuccessImage("https://placehold.co/600x400?text=AI+Donusum+Resmi+Burada+Gozukecek");

            try
            {
                var payload = new
                {
                    instances = new { prompt = $"A realistic fitness transformation photo of a person based on this image, looking {targetDescription}. High quality, gym setting." },
                    parameters = new { sampleCount = 1 }
                };

                var response = await _httpClient.PostAsync($"{ImageModelUrl}?key={_apiKey}",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var resJson = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(resJson);
                    var b64 = doc.RootElement.GetProperty("predictions")[0].GetProperty("bytesBase64Encoded").GetString();
                    return GeminiResult.SuccessImage($"data:image/png;base64,{b64}");
                }
                return GeminiResult.Failure("Görsel oluşturulamadı.");
            }
            catch
            {
                return GeminiResult.Failure("Görselleştirme servisinde hata.");
            }
        }

        private async Task<GeminiResult> GetMockData(int h, int w, string bt)
        {
            await Task.Delay(500);
            var recs = new List<ExerciseRecommendation> { new ExerciseRecommendation { Title = "Hafif Koşu", Description = "30 dk tempo.", DurationMinutes = 30, FocusArea = "Kardiyo", BodyType = bt, Goal = "Genel" } };
            return GeminiResult.Success(recs, "Günlük 2500 kalori beslenin.", "3 ay sonra daha dinç görüneceksiniz.", null);
        }
    }
}