using FitnessCenterManagement.Models;
using System.Collections.Generic;

namespace FitnessCenterManagement.Services
{
    // API çağrısından dönen sonucu sarmalamak için genel bir sınıf
    public class GeminiResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }

        // Plan önerisi için
        public IEnumerable<ExerciseRecommendation>? Recommendations { get; set; }
        public List<object>? Sources { get; set; } // Kaynaklar (URI ve Title içerir)

        // Görselleştirme için (Base64 imajı veya URL)
        public string? Description { get; set; }

        public static GeminiResult Success(IEnumerable<ExerciseRecommendation> recommendations, List<object>? sources = null)
        {
            return new GeminiResult { IsSuccess = true, Recommendations = recommendations, Sources = sources };
        }
        public static GeminiResult SuccessImage(string description)
        {
            return new GeminiResult { IsSuccess = true, Description = description };
        }
        public static GeminiResult Failure(string errorMessage)
        {
            return new GeminiResult { IsSuccess = false, ErrorMessage = errorMessage };
        }
    }
}