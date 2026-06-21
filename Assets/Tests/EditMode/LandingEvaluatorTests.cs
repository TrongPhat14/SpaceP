using NUnit.Framework;

namespace SpaceP.Scoring.Tests
{
    public sealed class LandingEvaluatorTests
    {
        private static readonly LandingScoringSettings Settings =
            LandingScoringSettings.Default;

        [Test]
        public void TerrainCollision_FailsWithoutScore()
        {
            LandingResult result = Evaluate(
                isLandingArea: false,
                speed: 1f,
                uprightness: 1f);

            Assert.That(result.Type, Is.EqualTo(LandingType.WrongLandingArea));
            Assert.That(result.Score, Is.Zero);
            Assert.That(result.ScoreMultiplier, Is.Zero);
        }

        [Test]
        public void SpeedAboveLimit_FailsWithoutScore()
        {
            LandingResult result = Evaluate(speed: 4.01f, uprightness: 1f);

            Assert.That(result.Type, Is.EqualTo(LandingType.TooFast));
            Assert.That(result.Score, Is.Zero);
        }

        [Test]
        public void UprightnessBelowLimit_FailsWithoutScore()
        {
            LandingResult result = Evaluate(speed: 1f, uprightness: 0.899f);

            Assert.That(result.Type, Is.EqualTo(LandingType.TooSteep));
            Assert.That(result.Score, Is.Zero);
        }

        [Test]
        public void ExactFailureThresholds_SucceedWithMinimumScore()
        {
            LandingResult result = Evaluate(speed: 4f, uprightness: 0.9f);

            Assert.That(result.Type, Is.EqualTo(LandingType.Success));
            Assert.That(result.Score, Is.EqualTo(100));
        }

        [Test]
        public void PerfectLanding_ReturnsMaximumScoreTimesMultiplier()
        {
            LandingResult result = Evaluate(
                speed: 0f,
                uprightness: 1f,
                multiplier: 5);

            Assert.That(result.Score, Is.EqualTo(2500));
            Assert.That(result.SpeedQuality, Is.EqualTo(1f));
            Assert.That(result.AngleQuality, Is.EqualTo(1f));
        }

        [Test]
        public void MidQualityLanding_UsesConfiguredWeights()
        {
            LandingResult result = Evaluate(speed: 2f, uprightness: 0.95f);

            Assert.That(result.SpeedQuality, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(result.AngleQuality, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(result.Score, Is.EqualTo(300));
        }

        [Test]
        public void InvalidMultiplier_IsClampedToOne()
        {
            LandingResult result = Evaluate(
                speed: 0f,
                uprightness: 1f,
                multiplier: 0);

            Assert.That(result.ScoreMultiplier, Is.EqualTo(1));
            Assert.That(result.Score, Is.EqualTo(500));
        }

        [Test]
        public void StabilizerUpgrade_ImprovesSameLandingScore()
        {
            LandingResult baseResult = Evaluate(
                speed: 3f,
                uprightness: 0.92f,
                safeSpeedLimit: 4f,
                minimumUprightness: 0.9f);

            LandingResult upgradedResult = Evaluate(
                speed: 3f,
                uprightness: 0.92f,
                safeSpeedLimit: 4.9f,
                minimumUprightness: 0.84f);

            Assert.That(upgradedResult.Score, Is.GreaterThan(baseResult.Score));
        }

        private static LandingResult Evaluate(
            bool isLandingArea = true,
            float speed = 0f,
            float uprightness = 1f,
            float safeSpeedLimit = 4f,
            float minimumUprightness = 0.9f,
            int multiplier = 1)
        {
            LandingAttempt attempt = new LandingAttempt(
                isLandingArea,
                speed,
                uprightness,
                safeSpeedLimit,
                minimumUprightness,
                multiplier);

            return LandingEvaluator.Evaluate(attempt, Settings);
        }
    }
}
