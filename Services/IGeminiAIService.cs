using FitnessCenterManagement.Models;
using System.Collections.Generic;

namespace FitnessCenterManagement.Services
{
    public interface IGeminiAIService
    {
        Task<GeminiResult> GetExerciseRecommendations(int heightCm, int weightKg, string bodyType, string? imageBase64);
        Task<GeminiResult> GenerateTransformationVisualization(string base64Image, string targetDescription);
    }
}