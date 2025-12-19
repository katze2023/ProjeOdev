using System;
using System.Collections.Generic;

namespace FitnessCenterManagement.Services
{
    /// <summary>
    /// Kullanıcı verilerine göre otomatik diyet ve spor programı üreten servis.
    /// </summary>
    public class FitnessDemoDataService
    {
        public class ProgramResult
        {
            public string Goal { get; set; }
            public double BMI { get; set; }
            public string BmiCategory { get; set; }
            public string DietPlan { get; set; }
            public List<WorkoutDay> WorkoutProgram { get; set; }
        }

        public class WorkoutDay
        {
            public int DayNumber { get; set; }
            public string Focus { get; set; }
            public List<string> Exercises { get; set; }
        }

        /// <summary>
        /// Boy (cm), Kilo (kg) ve Vücut Tipi bilgilerine göre program oluşturur.
        /// </summary>
        public ProgramResult GenerateProgram(double heightCm, double weightKg, string bodyType)
        {
            double heightMeters = heightCm / 100;
            double bmi = weightKg / (heightMeters * heightMeters);

            string goal;
            string bmiCategory;

            // BMI ve hedefin belirlenmesi
            if (bmi < 18.5)
            {
                goal = "Kilo Alma";
                bmiCategory = "Zayıf";
            }
            else if (bmi >= 18.5 && bmi < 25)
            {
                goal = "Hacim Kazanma";
                bmiCategory = "Normal";
            }
            else
            {
                goal = "Kilo Verme";
                bmiCategory = "Fazla Kilolu";
            }

            return new ProgramResult
            {
                BMI = Math.Round(bmi, 2),
                BmiCategory = bmiCategory,
                Goal = goal,
                DietPlan = GetDietPlan(goal, bodyType),
                WorkoutProgram = GetWorkoutProgram(goal)
            };
        }

        private string GetDietPlan(string goal, string bodyType)
        {
            if (goal == "Kilo Verme")
            {
                return "Düşük Karbonhidrat - Yüksek Protein Diyeti: \n" +
                       "- Sabah: 3 yumurta akı, 1 tam yumurta, bol yeşillik.\n" +
                       "- Öğle: 150g Izgara Tavuk, Mevsim salata (yağsız).\n" +
                       "- Akşam: 150g Izgara Balık veya Sebze yemeği, 3 kaşık yoğurt.\n" +
                       "- Ara: 10 adet çiğ badem.";
            }
            else if (goal == "Kilo Alma")
            {
                return "Yüksek Kalori - Kompleks Karbonhidrat Diyeti: \n" +
                       "- Sabah: 4 yumurta, 100g yulaf ezmesi, fıstık ezmesi.\n" +
                       "- Öğle: 200g Kırmızı et, 1 bardak pirinç pilavı, ayran.\n" +
                       "- Akşam: 200g Tavuk, haşlanmış patates, salata.\n" +
                       "- Ara: Muz ve whey protein veya süt.";
            }
            else // Hacim Kazanma
            {
                return "Clean Bulking (Temiz Hacim) Diyeti: \n" +
                       "- Sabah: 3 yumurta, lor peyniri, tam buğday ekmeği.\n" +
                       "- Öğle: 150g Hindi füme/göğüs, bulgur pilavı, yeşil salata.\n" +
                       "- Akşam: 150g Izgara Et/Tavuk, az miktar makarna, yoğurt.\n" +
                       "- Ara: Elma ve 2 tam ceviz.";
            }
        }

        private List<WorkoutDay> GetWorkoutProgram(string goal)
        {
            var program = new List<WorkoutDay>();

            if (goal == "Kilo Verme")
            {
                program.Add(new WorkoutDay { DayNumber = 1, Focus = "Tüm Vücut & Kardiyo", Exercises = new List<string> { "20 dk HIIT Koşu", "Push-up 3x12", "Bodyweight Squat 3x20", "Plank 3x45sn" } });
                program.Add(new WorkoutDay { DayNumber = 2, Focus = "Aktif Dinlenme", Exercises = new List<string> { "45 dk Tempolu Yürüyüş", "Esnetme Hareketleri" } });
                program.Add(new WorkoutDay { DayNumber = 3, Focus = "Alt Vücut & Karın", Exercises = new List<string> { "Lunge 3x15", "Glute Bridge 3x20", "Leg Raise 3x15", "15 dk Bisiklet" } });
            }
            else if (goal == "Kilo Alma")
            {
                program.Add(new WorkoutDay { DayNumber = 1, Focus = "Üst Vücut (Ağır)", Exercises = new List<string> { "Bench Press 4x8", "Bent Over Row 4x8", "Military Press 3x10", "Bicep Curl 3x12" } });
                program.Add(new WorkoutDay { DayNumber = 2, Focus = "Alt Vücut (Ağır)", Exercises = new List<string> { "Back Squat 4x8", "Deadlift 3x5", "Leg Press 3x10", "Calf Raise 4x15" } });
                program.Add(new WorkoutDay { DayNumber = 3, Focus = "Hipertrofi Karma", Exercises = new List<string> { "Incline Dumbbell Press 3x12", "Lat Pulldown 3x12", "Leg Extension 3x15", "Triceps Pushdown 3x12" } });
            }
            else // Hacim Kazanma
            {
                program.Add(new WorkoutDay { DayNumber = 1, Focus = "İtiş Günü", Exercises = new List<string> { "Bench Press 3x10", "Shoulder Press 3x10", "Dips 3xMax", "Lateral Raise 3x15" } });
                program.Add(new WorkoutDay { DayNumber = 2, Focus = "Çekiş Günü", Exercises = new List<string> { "Pull-ups 3xMax", "Seated Row 3x12", "Face Pull 3x15", "Hammer Curl 3x12" } });
                program.Add(new WorkoutDay { DayNumber = 3, Focus = "Bacak Günü", Exercises = new List<string> { "Squat 3x10", "Romanian Deadlift 3x12", "Walking Lunges 3x20 adım", "Plank 3x1dk" } });
            }

            return program;
        }
    }
}