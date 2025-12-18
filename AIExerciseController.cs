using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FitnessCenterManagement.Models;
using FitnessCenterManagement.Models.ViewModels;
using FitnessCenterManagement.Services;

namespace FitnessCenterManagement.Controllers
{
    [Authorize] // Giriş yapmış kullanıcılar erişebilir
    public class AIExerciseController : Controller
    {
        private readonly IGeminiAIService _geminiService;
        private readonly ILogger<AIExerciseController> _logger;

        public AIExerciseController(IGeminiAIService geminiService, ILogger<AIExerciseController> logger)
        {
            _geminiService = geminiService;
            _logger = logger;
        }

        // GET: AIExercise/Index - Form gösterimi
        [HttpGet]
        public IActionResult Index()
        {
            var model = new AIUserInputViewModel
            {
                HeightCm = 180,
                WeightKg = 75,
                BodyType = "Mesomorph"
            };
            return View(model);
        }

        // POST: AIExercise/GetRecommendations - AJAX API çağrısı
        [HttpPost]
        public async Task<IActionResult> GetRecommendations([FromForm] AIUserInputViewModel model)
        {
            _logger.LogInformation("GetRecommendations çağrıldı");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("Model validation hatası: {Errors}", string.Join(", ", errors));
                return BadRequest(new { success = false, error = string.Join(", ", errors) });
            }

            string? imageBase64 = null;

            // Resim yüklendiyse Base64'e çevir
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                _logger.LogInformation("Resim yüklendi: {FileName}, Boyut: {Size}", model.ImageFile.FileName, model.ImageFile.Length);

                // Dosya boyutu kontrolü (max 5MB)
                if (model.ImageFile.Length > 5 * 1024 * 1024)
                {
                    _logger.LogWarning("Dosya boyutu çok büyük: {Size}", model.ImageFile.Length);
                    return BadRequest(new { success = false, error = "Fotoğraf boyutu 5MB'dan küçük olmalıdır." });
                }

                // Dosya tipi kontrolü
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
                if (!allowedTypes.Contains(model.ImageFile.ContentType.ToLower()))
                {
                    _logger.LogWarning("Geçersiz dosya tipi: {ContentType}", model.ImageFile.ContentType);
                    return BadRequest(new { success = false, error = "Sadece JPEG ve PNG formatları desteklenmektedir." });
                }

                using (var ms = new MemoryStream())
                {
                    await model.ImageFile.CopyToAsync(ms);
                    imageBase64 = Convert.ToBase64String(ms.ToArray());
                    _logger.LogInformation("Resim Base64'e çevrildi. Uzunluk: {Length}", imageBase64.Length);
                }
            }

            try
            {
                _logger.LogInformation("Gemini AI servisi çağrılıyor...");

                // Gemini AI servisi çağrısı
                var result = await _geminiService.GetExerciseRecommendations(
                    model.HeightCm,
                    model.WeightKg,
                    model.BodyType,
                    imageBase64
                );

                if (!result.IsSuccess)
                {
                    _logger.LogError("AI servisi başarısız: {Error}", result.ErrorMessage);
                    return BadRequest(new { success = false, error = result.ErrorMessage });
                }

                _logger.LogInformation("AI servisi başarılı. Öneri sayısı: {Count}", result.Recommendations?.Count() ?? 0);

                return Ok(new
                {
                    success = true,
                    recommendations = result.Recommendations,
                    sources = result.Sources
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetRecommendations'da hata oluştu");
                return StatusCode(500, new { success = false, error = $"Sunucu hatası: {ex.Message}" });
            }
        }

        // GET: AIExercise/Result - Sonuçları göster (opsiyonel)
        [HttpGet]
        public IActionResult Result(IEnumerable<ExerciseRecommendation>? recommendations)
        {
            if (recommendations == null || !recommendations.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            return View(recommendations);
        }
    }
}