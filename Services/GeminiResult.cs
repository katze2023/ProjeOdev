using FitnessCenterManagement.Models;
using System.Collections.Generic;

namespace FitnessCenterManagement.Services
{
    public class GeminiResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }

        // Egzersiz planı listesi
        public IEnumerable<ExerciseRecommendation>? Recommendations { get; set; }

        // Diyet Planı metni
        public string? DietPlan { get; set; }

        // 3 Aylık Dönüşüm Öngörüsü metni
        public string? TransformationPrediction { get; set; }

        public List<object>? Sources { get; set; }

        public string? GeneratedImageUrl { get; set; }

        public static GeminiResult Success(IEnumerable<ExerciseRecommendation> recommendations, string dietPlan, string transformationPrediction, List<object>? sources = null)
        {
            return new GeminiResult
            {
                IsSuccess = true,
                Recommendations = recommendations,
                DietPlan = dietPlan,
                TransformationPrediction = transformationPrediction,
                Sources = sources
            };
        }

        public static GeminiResult SuccessImage(string imageUrl)
        {
            return new GeminiResult { IsSuccess = true, GeneratedImageUrl = imageUrl };
        }

        public static GeminiResult Failure(string errorMessage)
        {
            return new GeminiResult { IsSuccess = false, ErrorMessage = errorMessage };
        }
    }
}