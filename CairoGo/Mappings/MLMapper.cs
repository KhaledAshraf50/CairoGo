using CairoGo.DTOs.MLIntegration;
using CairoGo.Models.Entity;

namespace CairoGo.Mappings
{
    public static class MLMapper
    {
        public static List<string> ToMLAnswers(PreferenceProfile profile)
        {
            var answers = new List<string>();

            // 1. TravelVibe
            answers.Add(profile.TravelVibe.ToString());

            // 2. WeatherPref
            answers.Add(profile.WeatherPref.ToString());

            // 3. Budget (نحوله لـ CostTier text)
            string costTier = profile.Budget switch
            {
                //<= 300 => "Low",
                //<= 1000 => "Medium",
                //_ => "High"
                <= 500 => "Budget",
                <= 2000 => "Mid",
                _ => "Luxury"
            };
            answers.Add(costTier);

            // 4. ActivityTypes - نضيفهم كلهم
            foreach (var activity in profile.ActivityTypes)
            {
                answers.Add(activity.Name.ToString());
            }

            // 5. TripDays - مهم جداً للـ ML (عدد الأيام)
            answers.Add(profile.TripDays.ToString());

            return answers;
        }       
        public static MLRecommendationDto ToDto(this MLRecommendationDto mlRec)
        {
            return new MLRecommendationDto
            {
                Name = mlRec.Name,
                Category = mlRec.Category,
                Final_Score = mlRec.Final_Score
            };
        }

    }
}
