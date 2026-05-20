using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private const float GRAVITY_NORMAL = 0.7f;

    public static PlayerMovement instance { get; private set; }

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
        public LandingType landingType;
        public int score;
        public float dotVector;
        public float speed;
        public int scoreMultiplier;
    }

    public class OnStateChangeEventArgs : EventArgs
    {
        public State State;
    }

    public enum LandingType
    {
        Success,
        WrongLandingArea,
        TooSpeedLanding,
        TooSteepAngle,
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

    private bool hasLandingResult;

    private void Awake()
    {
        instance = this;

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
        onBeforeForce?.Invoke(this, EventArgs.Empty);

        switch (state)
        {
            default:
            case State.WaitingToStart:
                if (GameInput.Instance.IsUpActionPressed() ||
                    GameInput.Instance.IsRightActionPressed() ||
                    GameInput.Instance.IsLeftActionPressed() ||
                    GameInput.Instance.GetMovementInputVector2() != Vector2.zero)
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
                    GameInput.Instance.GetMovementInputVector2() != Vector2.zero)
                {
                    ConsumeFuel();
                }

                float gamePadDeadZone = .4f;

                if (GameInput.Instance.IsUpActionPressed() ||
                    GameInput.Instance.GetMovementInputVector2().y > gamePadDeadZone)
                {
                    // STORE CHANGED:
                    // Engine force lấy từ UpgradeManager.
                    float force = UpgradeManager.GetEngineForce();

                    rb.AddForce(force * transform.up * Time.deltaTime);
                    onUpForce?.Invoke(this, EventArgs.Empty);
                }

                if (GameInput.Instance.IsLeftActionPressed())
                {
                    // STORE CHANGED:
                    // Turn speed lấy từ UpgradeManager.
                    float turnSpeed = UpgradeManager.GetTurnSpeed();

                    rb.AddTorque(+turnSpeed * Time.deltaTime);
                    onLeftForce?.Invoke(this, EventArgs.Empty);
                }

                if (GameInput.Instance.IsRightActionPressed())
                {
                    // STORE CHANGED:
                    // Turn speed lấy từ UpgradeManager.
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

        if (!collision.gameObject.TryGetComponent(out LandingPlace landingPlace))
        {
            Debug.Log("Crashed on the Terrain");

            hasLandingResult = true;

            onLanded?.Invoke(this, new OnLandedEventArgs
            {
                landingType = LandingType.WrongLandingArea,
                dotVector = 0f,
                speed = 0f,
                scoreMultiplier = 0,
                score = 0,
            });

            SetState(State.GameOver);
            return;
        }

        // STORE CHANGED:
        // Tốc độ hạ cánh cho phép lấy từ Landing Stabilizer upgrade.
        float softLandingVelocityMagnitude = UpgradeManager.GetSoftLandingVelocityMagnitude();

        float relaticeVelocityMagnitude = collision.relativeVelocity.magnitude;

        if (relaticeVelocityMagnitude > softLandingVelocityMagnitude)
        {
            Debug.Log("Landed was high");

            hasLandingResult = true;

            onLanded?.Invoke(this, new OnLandedEventArgs
            {
                landingType = LandingType.TooSpeedLanding,
                dotVector = 0f,
                speed = relaticeVelocityMagnitude,
                scoreMultiplier = 0,
                score = 0,
            });

            SetState(State.GameOver);
            return;
        }

        float dotVector = Vector2.Dot(Vector2.up, transform.up);

        // STORE CHANGED:
        // Góc hạ cánh cho phép lấy từ Landing Stabilizer upgrade.
        float minDotVector = UpgradeManager.GetMinLandingDotVector();

        if (dotVector < minDotVector)
        {
            Debug.Log("Landed on a too steep angle");

            hasLandingResult = true;

            onLanded?.Invoke(this, new OnLandedEventArgs
            {
                landingType = LandingType.TooSteepAngle,
                dotVector = dotVector,
                speed = relaticeVelocityMagnitude,
                scoreMultiplier = 0,
                score = 0,
            });

            SetState(State.GameOver);
            return;
        }

        Debug.Log("Successfull landed");

        float maxScoreAmountLandingAngle = 100;
        float scoreDotVectorMutiplier = 10f;
        float landingAngleScore = maxScoreAmountLandingAngle - Mathf.Abs(dotVector - 1f) * scoreDotVectorMutiplier * maxScoreAmountLandingAngle;

        float maxScoreAmountLandingSpeed = 100;
        float landingSpeedScore = (softLandingVelocityMagnitude - relaticeVelocityMagnitude) * maxScoreAmountLandingSpeed;

        Debug.Log("Landing Angle Score" + landingAngleScore);
        Debug.Log("Landing speed Score" + landingSpeedScore);

        int score = Mathf.RoundToInt((landingAngleScore + landingSpeedScore) * landingPlace.GetScoreMultiplier());

        Debug.Log("Score :" + score);

        hasLandingResult = true;

        StopLanderAfterSuccessLanding();

        onLanded?.Invoke(this, new OnLandedEventArgs
        {
            landingType = LandingType.Success,
            dotVector = dotVector,
            speed = relaticeVelocityMagnitude,
            scoreMultiplier = landingPlace.GetScoreMultiplier(),
            score = score,
        });

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
        return fuelAmount / fuelAmountMax;
    }

    private void ConsumeFuel()
    {
        float fuelConsumeAmount = 1f;
        fuelAmount -= fuelConsumeAmount * Time.deltaTime;
    }

    public float GetFuel()
    {
        return fuelAmount;
    }
}