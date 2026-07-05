using System;
using SpaceP.Scoring;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private const float GRAVITY_NORMAL = 0.7f;
    private const float GAMEPAD_DEAD_ZONE = 0.4f;
    private const float FUEL_CONSUME_PER_SECOND = 1f;
    private const float FUEL_PICKUP_AMOUNT = 10f;

    public static PlayerMovement Instance { get; private set; }

    public event EventHandler onUpForce;
    public event EventHandler onLeftForce;
    public event EventHandler onRightForce;
    public event EventHandler onBeforeForce;
    public event EventHandler<OnStateChangeEventArgs> onStateChange;
    public event EventHandler onCoinPickUp;
    public event EventHandler onFuelPickUp;
    public event EventHandler onWindForce;
    public event EventHandler<OnLandedEventArgs> onLanded;

    [Header("Landing Scoring")]
    [SerializeField] private LandingScoringConfig landingScoringConfig;

    private Rigidbody2D rb;
    private float fuelAmount;
    private float fuelAmountMax;
    private State state;
    private bool hasLandingResult;
    private bool tutorialControlLocked;
    private RigidbodyConstraints2D constraintsBeforeTutorial;
    private float gravityScaleBeforeTutorial;

    public enum State
    {
        Normal,
        WaitingToStart,
        GameOver,
    }

    public class OnLandedEventArgs : EventArgs
    {
        public OnLandedEventArgs(LandingResult result)
        {
            Result = result;
        }

        public LandingResult Result { get; }
    }

    public class OnStateChangeEventArgs : EventArgs
    {
        public State State;
    }

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();

        WarnIfScoringConfigMissing();
        ResetRuntimeState();
    }

    private void FixedUpdate()
    {
        if (tutorialControlLocked)
        {
            return;
        }

        onBeforeForce?.Invoke(this, EventArgs.Empty);

        PlayerInputSnapshot input = ReadInput();

        switch (state)
        {
            case State.WaitingToStart:
                UpdateWaitingToStart(input);
                break;

            case State.Normal:
                UpdateMovement(input);
                break;

            case State.GameOver:
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLandingResult)
        {
            return;
        }

        LandingResult result = EvaluateLanding(collision);
        hasLandingResult = true;

        if (result.IsSuccess)
        {
            StopLanderAfterSuccessLanding();
        }

        ReleaseLog.Log(
            $"Landing result={result.Type} speed={result.ImpactSpeed:0.00} " +
            $"uprightness={result.Uprightness:0.000} score={result.Score}");

        onLanded?.Invoke(this, new OnLandedEventArgs(result));
        SetState(State.GameOver);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasLandingResult)
        {
            return;
        }

        if (collision.TryGetComponent(out FuelPickUp fuel))
        {
            AddFuel(FUEL_PICKUP_AMOUNT);
            onFuelPickUp?.Invoke(this, EventArgs.Empty);
            fuel.DestroyFuel();
            return;
        }

        if (collision.TryGetComponent(out CoinPickUp coin))
        {
            onCoinPickUp?.Invoke(this, EventArgs.Empty);
            coin.DestroyCoin();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (hasLandingResult || !collision.TryGetComponent(out WindForce wind))
        {
            return;
        }

        onWindForce?.Invoke(this, EventArgs.Empty);
        rb.AddForce(wind.GetDirection() * wind.GetStrength(), ForceMode2D.Force);
    }

    private void ResetRuntimeState()
    {
        fuelAmountMax = UpgradeManager.GetFuelAmountMax();
        fuelAmount = fuelAmountMax;
        hasLandingResult = false;
        rb.gravityScale = 0f;
        SetState(State.WaitingToStart);
    }

    private PlayerInputSnapshot ReadInput()
    {
        if (GameInput.Instance == null)
        {
            return PlayerInputSnapshot.Empty;
        }

        Vector2 movement = GameInput.Instance.GetMovementInputVector2();

        return new PlayerInputSnapshot(
            GameInput.Instance.IsUpActionPressed() || movement.y > GAMEPAD_DEAD_ZONE,
            GameInput.Instance.IsLeftActionPressed() || movement.x < -GAMEPAD_DEAD_ZONE,
            GameInput.Instance.IsRightActionPressed() || movement.x > GAMEPAD_DEAD_ZONE,
            movement != Vector2.zero);
    }

    private void UpdateWaitingToStart(PlayerInputSnapshot input)
    {
        if (!input.HasAnyInput)
        {
            return;
        }

        ConsumeFuel();
        rb.gravityScale = GRAVITY_NORMAL;
        SetState(State.Normal);
    }

    private void UpdateMovement(PlayerInputSnapshot input)
    {
        if (fuelAmount <= 0f)
        {
            return;
        }

        if (input.HasAnyInput)
        {
            ConsumeFuel();
        }

        if (input.Thrust)
        {
            ApplyThrust();
        }

        if (input.RotateLeft)
        {
            ApplyTorque(+UpgradeManager.GetTurnSpeed());
            onLeftForce?.Invoke(this, EventArgs.Empty);
        }

        if (input.RotateRight)
        {
            ApplyTorque(-UpgradeManager.GetTurnSpeed());
            onRightForce?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ApplyThrust()
    {
        rb.AddForce(UpgradeManager.GetEngineForce() * Time.deltaTime * transform.up);
        onUpForce?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyTorque(float torque)
    {
        rb.AddTorque(torque * Time.deltaTime);
    }

    private LandingResult EvaluateLanding(Collision2D collision)
    {
        bool isLandingArea = collision.gameObject.TryGetComponent(out LandingPlace landingPlace);
        int scoreMultiplier = isLandingArea ? landingPlace.GetScoreMultiplier() : 0;

        LandingAttempt attempt = new LandingAttempt(
            isLandingArea,
            collision.relativeVelocity.magnitude,
            Vector2.Dot(Vector2.up, transform.up),
            UpgradeManager.GetSoftLandingVelocityMagnitude(),
            UpgradeManager.GetMinLandingDotVector(),
            scoreMultiplier);

        LandingScoringSettings settings = landingScoringConfig != null
            ? landingScoringConfig.GetSettings()
            : LandingScoringSettings.Default;

        return LandingEvaluator.Evaluate(attempt, settings);
    }

    private void StopLanderAfterSuccessLanding()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void SetState(State nextState)
    {
        if (state == nextState)
        {
            return;
        }

        state = nextState;

        onStateChange?.Invoke(this, new OnStateChangeEventArgs
        {
            State = state
        });
    }

    private void ConsumeFuel()
    {
        fuelAmount = Mathf.Max(0f, fuelAmount - FUEL_CONSUME_PER_SECOND * Time.fixedDeltaTime);
    }

    private void AddFuel(float amount)
    {
        fuelAmount = Mathf.Clamp(fuelAmount + amount, 0f, fuelAmountMax);
    }

    private void WarnIfScoringConfigMissing()
    {
        if (landingScoringConfig != null)
        {
            return;
        }

        ReleaseLog.Warning(
            $"{nameof(PlayerMovement)} on '{name}' has no {nameof(LandingScoringConfig)} assigned. " +
            "Default landing scoring settings will be used.",
            this);
    }

    public float GetFuelAmountNormalized()
    {
        if (fuelAmountMax <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(fuelAmount / fuelAmountMax);
    }

    public float GetFuel()
    {
        return fuelAmount;
    }

    public void SetTutorialControlLocked(bool isLocked)
    {
        if (rb == null || tutorialControlLocked == isLocked)
        {
            return;
        }

        tutorialControlLocked = isLocked;

        if (isLocked)
        {
            constraintsBeforeTutorial = rb.constraints;
            gravityScaleBeforeTutorial = rb.gravityScale;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            return;
        }

        rb.constraints = constraintsBeforeTutorial;
        rb.gravityScale = gravityScaleBeforeTutorial;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private struct PlayerInputSnapshot
    {
        public static readonly PlayerInputSnapshot Empty = new PlayerInputSnapshot(false, false, false, false);

        public PlayerInputSnapshot(bool thrust, bool rotateLeft, bool rotateRight, bool analogMovement)
        {
            Thrust = thrust;
            RotateLeft = rotateLeft;
            RotateRight = rotateRight;
            AnalogMovement = analogMovement;
        }

        public bool Thrust { get; }
        public bool RotateLeft { get; }
        public bool RotateRight { get; }
        public bool AnalogMovement { get; }
        public bool HasAnyInput => Thrust || RotateLeft || RotateRight || AnalogMovement;
    }
}
