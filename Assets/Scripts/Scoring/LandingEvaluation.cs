namespace SpaceP.Scoring
{
    public enum LandingType
    {
        Success,
        WrongLandingArea,
        TooFast,
        TooSteep,
    }

    public readonly struct LandingAttempt
    {
        public LandingAttempt(
            bool isLandingArea,
            float impactSpeed,
            float uprightness,
            float safeSpeedLimit,
            float minimumUprightness,
            int scoreMultiplier)
        {
            IsLandingArea = isLandingArea;
            ImpactSpeed = impactSpeed;
            Uprightness = uprightness;
            SafeSpeedLimit = safeSpeedLimit;
            MinimumUprightness = minimumUprightness;
            ScoreMultiplier = scoreMultiplier;
        }

        public bool IsLandingArea { get; }
        public float ImpactSpeed { get; }
        public float Uprightness { get; }
        public float SafeSpeedLimit { get; }
        public float MinimumUprightness { get; }
        public int ScoreMultiplier { get; }
    }

    public readonly struct LandingResult
    {
        public LandingResult(
            LandingType type,
            int score,
            float impactSpeed,
            float uprightness,
            int scoreMultiplier,
            float speedQuality,
            float angleQuality)
        {
            Type = type;
            Score = score;
            ImpactSpeed = impactSpeed;
            Uprightness = uprightness;
            ScoreMultiplier = scoreMultiplier;
            SpeedQuality = speedQuality;
            AngleQuality = angleQuality;
        }

        public LandingType Type { get; }
        public int Score { get; }
        public float ImpactSpeed { get; }
        public float Uprightness { get; }
        public int ScoreMultiplier { get; }
        public float SpeedQuality { get; }
        public float AngleQuality { get; }

        public bool IsSuccess => Type == LandingType.Success;
    }
}
