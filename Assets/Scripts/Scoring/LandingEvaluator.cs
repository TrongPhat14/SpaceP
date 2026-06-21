using UnityEngine;

namespace SpaceP.Scoring
{
    public static class LandingEvaluator
    {
        public static LandingResult Evaluate(
            LandingAttempt attempt,
            LandingScoringSettings settings)
        {
            float impactSpeed = Mathf.Max(0f, attempt.ImpactSpeed);
            float uprightness = Mathf.Clamp(attempt.Uprightness, -1f, 1f);

            if (!attempt.IsLandingArea)
            {
                return Failed(LandingType.WrongLandingArea, impactSpeed, uprightness);
            }

            float safeSpeedLimit = Mathf.Max(Mathf.Epsilon, attempt.SafeSpeedLimit);
            float minimumUprightness = Mathf.Clamp(attempt.MinimumUprightness, -1f, 1f);

            if (impactSpeed > safeSpeedLimit)
            {
                return Failed(LandingType.TooFast, impactSpeed, uprightness);
            }

            if (uprightness < minimumUprightness)
            {
                return Failed(LandingType.TooSteep, impactSpeed, uprightness);
            }

            float speedQuality = 1f - Mathf.Clamp01(impactSpeed / safeSpeedLimit);
            float angleQuality = Mathf.InverseLerp(minimumUprightness, 1f, uprightness);
            float totalWeight = settings.SpeedWeight + settings.AngleWeight;

            float quality = totalWeight > Mathf.Epsilon
                ? (speedQuality * settings.SpeedWeight + angleQuality * settings.AngleWeight) / totalWeight
                : (speedQuality + angleQuality) * 0.5f;

            int baseScore = Mathf.RoundToInt(Mathf.Lerp(
                settings.MinimumSuccessScore,
                settings.MaximumSuccessScore,
                Mathf.Clamp01(quality)));

            int scoreMultiplier = Mathf.Max(1, attempt.ScoreMultiplier);
            long multipliedScore = (long)baseScore * scoreMultiplier;
            int finalScore = multipliedScore > int.MaxValue
                ? int.MaxValue
                : (int)multipliedScore;

            return new LandingResult(
                LandingType.Success,
                finalScore,
                impactSpeed,
                uprightness,
                scoreMultiplier,
                speedQuality,
                angleQuality);
        }

        private static LandingResult Failed(
            LandingType type,
            float impactSpeed,
            float uprightness)
        {
            return new LandingResult(
                type,
                0,
                impactSpeed,
                uprightness,
                0,
                0f,
                0f);
        }
    }
}
