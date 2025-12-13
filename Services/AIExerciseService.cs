using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManagement.Services
{
    public class AIExerciseService
    {
        private readonly ApplicationDbContext _context;

        public AIExerciseService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ExerciseRecommendation>> GetRecommendations(ApplicationUser user)
        {
            string bodyType = user.BodyType ?? "Athletic";
            string goal = DetermineGoal(user);

            var recommendations = await _context.ExerciseRecommendations
                .Where(x => x.BodyType == bodyType && x.Goal == goal)
                .ToListAsync();

            return recommendations;
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
    }
}
