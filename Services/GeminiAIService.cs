using FitnessCenterManagement.Models;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace FitnessCenterManagement.Services
{
    public class GeminiAIService : IGeminiAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _model;
        private readonly ILogger<GeminiAIService> _logger;

        public GeminiAIService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiAIService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // appsettings.json'dan GeminiAPI altındaki değerleri oku
            _apiKey = configuration["GeminiAPI:ApiKey"] ?? "";
            _baseUrl = configuration["GeminiAPI:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/models";
            _model = configuration["GeminiAPI:Model"] ?? "gemini-2.0-flash-exp";

            var timeout = int.TryParse(configuration["GeminiAPI:TimeoutSeconds"], out var t) ? t : 60;
            _httpClient.Timeout = TimeSpan.FromSeconds(timeout);

            _logger.LogInformation("GeminiAIService başlatıldı. Model: {Model}, API Key mevcut: {HasKey}", _model, !string.IsNullOrEmpty(_apiKey));
        }

        public async Task<GeminiResult> GetExerciseRecommendations(int heightCm, int weightKg, string bodyType, string? imageBase64)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogError("API anahtarı bulunamadı!");
                return GeminiResult.Failure("API anahtarı yapılandırılmamış. Lütfen appsettings.json dosyasında 'GeminiAPI:ApiKey' ayarını yapın.");
            }

            try
            {
                var bmi = (double)weightKg / Math.Pow((double)heightCm / 100, 2);
                var goal = bmi < 20 ? "Kas Geliştirme" : "Kilo Verme";

                _logger.LogInformation("Egzersiz önerisi oluşturuluyor. Boy: {Height}cm, Kilo: {Weight}kg, BMI: {BMI}, Hedef: {Goal}",
                    heightCm, weightKg, bmi.ToString("F2"), goal);

                var prompt = new StringBuilder();
                prompt.AppendLine($"Kullanıcının boyu {heightCm} cm, kilosu {weightKg} kg, vücut tipi {bodyType} (BMI: {bmi:F2}). Hedef: {goal}.");

                if (!string.IsNullOrEmpty(imageBase64))
                {
                    prompt.AppendLine("Yüklenen vücut fotoğrafını analiz ederek, hedefine yönelik 3 günlük egzersiz planı oluştur.");
                    _logger.LogInformation("Fotoğraf ile analiz yapılıyor");
                }
                else
                {
                    prompt.AppendLine("Bu verilere dayanarak, hedefine yönelik 3 günlük egzersiz planı oluştur.");
                }

                var parts = new List<object> { new { text = prompt.ToString() } };

                if (!string.IsNullOrEmpty(imageBase64))
                {
                    parts.Add(new
                    {
                        inline_data = new
                        {
                            mime_type = "image/jpeg",
                            data = imageBase64
                        }
                    });
                }

                var payload = new
                {
                    contents = new[]
                    {
                        new { role = "user", parts = parts.ToArray() }
                    },
                    generation_config = new
                    {
                        response_mime_type = "application/json",
                        response_schema = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    Title = new { type = "string" },
                                    Description = new { type = "string" },
                                    DurationMinutes = new { type = "integer" },
                                    FocusArea = new { type = "string" }
                                },
                                required = new[] { "Title", "Description", "DurationMinutes", "FocusArea" }
                            }
                        }
                    },
                    system_instruction = new
                    {
                        parts = new[]
                        {
                            new { text = "Sen sertifikalı bir fitness uzmanısın. Tüm yanıtlarını Türkçe ve JSON formatında üret." }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogDebug("API Request Payload: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var requestUrl = $"{_baseUrl}/{_model}:generateContent?key={_apiKey}";

                _logger.LogInformation("Gemini API çağrısı yapılıyor: {Url}", $"{_baseUrl}/{_model}:generateContent");

                var response = await _httpClient.PostAsync(requestUrl, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("API Response Status: {Status}", response.StatusCode);
                _logger.LogDebug("API Response Body: {Response}", responseJson);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API Hatası: {Status} - {Response}", response.StatusCode, responseJson);
                    return GeminiResult.Failure($"API Hatası: {response.StatusCode} - {responseJson}");
                }

                using (JsonDocument doc = JsonDocument.Parse(responseJson))
                {
                    var candidates = doc.RootElement.GetProperty("candidates");
                    if (candidates.GetArrayLength() == 0)
                    {
                        _logger.LogWarning("API'den yanıt alınamadı");
                        return GeminiResult.Failure("API'den yanıt alınamadı.");
                    }

                    var textContent = candidates[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    _logger.LogDebug("AI Text Response: {Text}", textContent);

                    var recommendations = JsonSerializer.Deserialize<List<ExerciseRecommendation>>(
                        textContent ?? "[]",
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    ) ?? new List<ExerciseRecommendation>();

                    foreach (var rec in recommendations)
                    {
                        rec.BodyType = bodyType;
                        rec.Goal = goal;
                    }

                    _logger.LogInformation("Başarıyla {Count} öneri oluşturuldu", recommendations.Count);

                    return GeminiResult.Success(recommendations);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP isteği başarısız oldu");
                return GeminiResult.Failure($"Ağ hatası: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "İstek zaman aşımına uğradı");
                return GeminiResult.Failure("İstek zaman aşımına uğradı. Lütfen tekrar deneyin.");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parse hatası");
                return GeminiResult.Failure($"Yanıt işlenirken hata oluştu: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklenmeyen hata");
                return GeminiResult.Failure($"Hata oluştu: {ex.Message}");
            }
        }

        public async Task<GeminiResult> GenerateTransformationVisualization(string base64Image, string targetDescription)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return GeminiResult.Failure("API anahtarı yapılandırılmamış.");
            }

            try
            {
                var prompt = $"Yüklenen vücut fotoğrafını analiz et ve '{targetDescription}' hedefine ulaştığında nasıl görüneceğini detaylı olarak açıkla. Gerçekçi bir tahmin yap.";

                var payload = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new object[]
                            {
                                new { text = prompt },
                                new
                                {
                                    inline_data = new
                                    {
                                        mime_type = "image/jpeg",
                                        data = base64Image
                                    }
                                }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var requestUrl = $"{_baseUrl}/{_model}:generateContent?key={_apiKey}";

                var response = await _httpClient.PostAsync(requestUrl, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Transformation API Hatası: {Status}", response.StatusCode);
                    return GeminiResult.Failure($"API Hatası: {response.StatusCode}");
                }

                using (JsonDocument doc = JsonDocument.Parse(responseJson))
                {
                    var textContent = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    return GeminiResult.SuccessImage(textContent ?? "Açıklama oluşturulamadı.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transformation hatası");
                return GeminiResult.Failure($"Hata: {ex.Message}");
            }
        }
    }
}