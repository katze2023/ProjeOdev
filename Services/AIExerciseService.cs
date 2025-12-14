using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;

namespace FitnessCenterManagement.Services
{
    public class AIExerciseService
    {
        public AIExerciseService(ApplicationDbContext context)
        {
        }

        public Task<List<ExerciseRecommendation>> GetRecommendations(ApplicationUser user)
        {
            string bodyType = user.BodyType ?? "Athletic";
            string goal = DetermineGoal(user);

            // Şu an sadece fallback AI kullanıyoruz
            return Task.FromResult(GetFallbackRecommendations(bodyType, goal));
        }

        private string DetermineGoal(ApplicationUser user)
        {
            if (user.WeightKg == null || user.HeightCm == null)
                return "Fit Kalma";

            double heightM = user.HeightCm.Value / 100.0;
            double bmi = user.WeightKg.Value / (heightM * heightM);

            if (bmi >= 25)
                return "Kilo Verme";
            else if (bmi < 20)
                return "Kas Geliştirme";
            else
                return "Fit Kalma";
        }

        private List<ExerciseRecommendation> GetFallbackRecommendations(string bodyType, string goal)
        {
            return new List<ExerciseRecommendation>
            {
                new ExerciseRecommendation
                {
                    Title = "Full Body Antrenman",
                    Description = "Tüm kas gruplarını çalıştıran dengeli bir egzersiz programı.",
                    BodyType = bodyType,
                    Goal = goal
                },
                new ExerciseRecommendation
                {
                    Title = "Kardiyo + Core",
                    Description = "Yağ yakımı ve core bölgesi güçlendirme egzersizleri.",
                    BodyType = bodyType,
                    Goal = goal
                }
            };
        }
    }
}
