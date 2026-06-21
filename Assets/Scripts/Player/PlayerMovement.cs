using System;
using SpaceP.Scoring;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private const float GRAVITY_NORMAL = 0.7f;

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

    public enum State
    {
        Normal,
        WaitingToStart,
        GameOver,
    }

    private Rigidbody2D rb;
    private float fuelAmount;
    private float fuelAmountMax = 10f;
    private State state;

    [Header("Landing Scoring")]
    [SerializeField] private LandingScoringConfig landingScoringConfig;

    private bool hasLandingResult;
    private bool tutorialControlLocked;
    private RigidbodyConstraints2D constraintsBeforeTutorial;
    private float gravityScaleBeforeTutorial;

    private void Awake()
    {
        Instance = this;

        if (landingScoringConfig == null)
        {
            Debug.LogWarning(
                $"{nameof(PlayerMovement)} on '{name}' has no {nameof(LandingScoringConfig)} assigned. " +
                "Default landing scoring settings will be used.",
                this);
        }

        // STORE CHANGED:
        // Fuel max lấy từ UpgradeManager thay vì hardcode 10f.
        fuelAmountMax = UpgradeManager.GetFuelAmountMax();

        fuelAmount = fuelAmountMax;
        state = State.WaitingToStart;

        LandingPlace landingPlace = GetComponent<LandingPlace>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        hasLandingResult = false;
    }

    private void FixedUpdate()
    {
        if (tutorialControlLocked)
        {
            return;
        }

        onBeforeForce?.Invoke(this, EventArgs.Empty);

        Vector2 movementInput = GameInput.Instance.GetMovementInputVector2();

        switch (state)
        {
            default:
            case State.WaitingToStart:
                if (GameInput.Instance.IsUpActionPressed() ||
                    GameInput.Instance.IsRightActionPressed() ||
                    GameInput.Instance.IsLeftActionPressed() ||
                    movementInput != Vector2.zero)
                {
                    ConsumeFuel();
                    rb.gravityScale = GRAVITY_NORMAL;
                    SetState(State.Normal);
                }
                break;

            case State.Normal:
                if (fuelAmount <= 0f)
                {
                    return;
                }

                if (GameInput.Instance.IsUpActionPressed() ||
                    GameInput.Instance.IsRightActionPressed() ||
                    GameInput.Instance.IsLeftActionPressed() ||
                    movementInput != Vector2.zero)
                {
                    ConsumeFuel();
                }

                float gamePadDeadZone = .4f;

                if (GameInput.Instance.IsUpActionPressed() ||
                    movementInput.y > gamePadDeadZone)
                {
                    float force = UpgradeManager.GetEngineForce();
                    rb.AddForce(force * transform.up * Time.deltaTime);
                    onUpForce?.Invoke(this, EventArgs.Empty);
                }

                if (GameInput.Instance.IsLeftActionPressed() ||
                    movementInput.x < -gamePadDeadZone)
                {
                    float turnSpeed = UpgradeManager.GetTurnSpeed();
                    rb.AddTorque(+turnSpeed * Time.deltaTime);
                    onLeftForce?.Invoke(this, EventArgs.Empty);
                }

                if (GameInput.Instance.IsRightActionPressed() ||
                    movementInput.x > gamePadDeadZone)
                {
                    float turnSpeed = UpgradeManager.GetTurnSpeed();
                    rb.AddTorque(-turnSpeed * Time.deltaTime);
                    onRightForce?.Invoke(this, EventArgs.Empty);
                }
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

        LandingResult result = LandingEvaluator.Evaluate(attempt, settings);

        hasLandingResult = true;

        if (result.IsSuccess)
        {
            StopLanderAfterSuccessLanding();
        }

        Debug.Log(
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

        if (collision.gameObject.TryGetComponent(out FuelPickUp fuel))
        {
            float addFuelAmount = 10f;
            fuelAmount += addFuelAmount;

            if (fuelAmount > fuelAmountMax)
            {
                fuelAmount = fuelAmountMax;
            }

            onFuelPickUp?.Invoke(this, EventArgs.Empty);
            fuel.DestroyFuel();
        }

        if (collision.gameObject.TryGetComponent(out CoinPickUp coin))
        {
            onCoinPickUp?.Invoke(this, EventArgs.Empty);
            coin.DestroyCoin();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (hasLandingResult)
        {
            return;
        }

        if (collision.gameObject.TryGetComponent(out WindForce wind))
        {
            onWindForce?.Invoke(this, EventArgs.Empty);
            rb.AddForce(wind.GetDirection() * wind.GetStrength(), ForceMode2D.Force);
        }
    }

    private void StopLanderAfterSuccessLanding()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void SetState(State state)
    {
        this.state = state;

        onStateChange?.Invoke(this, new OnStateChangeEventArgs
        {
            State = state
        });
    }

    public float GetFuelAmountNormalized()
    {
        if (fuelAmountMax <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(fuelAmount / fuelAmountMax);
    }

    private void ConsumeFuel()
    {
        float fuelConsumeAmount = 1f;
        fuelAmount = Mathf.Max(0f, fuelAmount - fuelConsumeAmount * Time.fixedDeltaTime);
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
}
