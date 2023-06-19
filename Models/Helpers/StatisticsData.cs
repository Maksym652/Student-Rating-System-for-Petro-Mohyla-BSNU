namespace StudentRatingSystemWebApp.Models
{
    public class StatisticsData
    {
        public float AveragePoint { get; set; }
        public List<float> AveragePointsOnCources { get; set; } = new();
        public float SumOfAdditionalPoints { get; set; }
        public float SumOfAdditionalPointsForPublicActivity { get; set; }
        public float SumOfAdditionalPointsForScientificWork { get; set; }
        public float SumOfAdditionalPoitnsForSportAchievments { get; set; }


        public string Description { get; set; } = "";

    }
}
