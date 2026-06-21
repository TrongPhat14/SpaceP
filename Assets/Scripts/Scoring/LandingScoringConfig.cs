using UnityEngine;

namespace SpaceP.Scoring
{
    [CreateAssetMenu(
        fileName = "LandingScoringConfig",
        menuName = "SpaceP/Landing Scoring Config")]
    public sealed class LandingScoringConfig : ScriptableObject
    {
        [SerializeField, Min(0)] private int minimumSuccessScore = 100;
        [SerializeField, Min(0)] private int maximumSuccessScore = 500;
        [SerializeField, Min(0f)] private float speedWeight = 0.6f;
        [SerializeField, Min(0f)] private float angleWeight = 0.4f;

        public LandingScoringSettings GetSettings()
        {
            return new LandingScoringSettings(
                minimumSuccessScore,
                maximumSuccessScore,
                speedWeight,
                angleWeight);
        }

        private void OnValidate()
        {
            minimumSuccessScore = Mathf.Max(0, minimumSuccessScore);
            maximumSuccessScore = Mathf.Max(minimumSuccessScore, maximumSuccessScore);
            speedWeight = Mathf.Max(0f, speedWeight);
            angleWeight = Mathf.Max(0f, angleWeight);

            if (speedWeight + angleWeight <= Mathf.Epsilon)
            {
                speedWeight = 0.6f;
                angleWeight = 0.4f;
            }
        }
    }

    public readonly struct LandingScoringSettings
    {
        public LandingScoringSettings(
            int minimumSuccessScore,
            int maximumSuccessScore,
            float speedWeight,
            float angleWeight)
        {
            MinimumSuccessScore = Mathf.Max(0, minimumSuccessScore);
            MaximumSuccessScore = Mathf.Max(MinimumSuccessScore, maximumSuccessScore);
            SpeedWeight = Mathf.Max(0f, speedWeight);
            AngleWeight = Mathf.Max(0f, angleWeight);
        }

        public int MinimumSuccessScore { get; }
        public int MaximumSuccessScore { get; }
        public float SpeedWeight { get; }
        public float AngleWeight { get; }

        public static LandingScoringSettings Default =>
            new LandingScoringSettings(100, 500, 0.6f, 0.4f);
    }
}
