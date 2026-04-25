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
        public State ste;
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


    private Rigidbody2D rd;
    private float fuelAmount;
    private float fuelAmountMax = 10f;
    private State state;



    private void Awake()
    {
        instance = this;
        fuelAmount = fuelAmountMax;
        state = State.WaitingToStart;
        LandingPlace landingPlace = GetComponent<LandingPlace>();
        rd = GetComponent<Rigidbody2D>();
        rd.gravityScale = 0f;

    }

    private void FixedUpdate()
    {

        onBeforeForce?.Invoke(this, EventArgs.Empty);

        switch (state)
        {
            default: 
            case State.WaitingToStart:
                if (Keyboard.current.upArrowKey.isPressed ||
                    Keyboard.current.leftArrowKey.isPressed ||
                    Keyboard.current.rightArrowKey.isPressed) { 
                    ConsumeFuel();
                    rd.gravityScale = GRAVITY_NORMAL;
                    SetState(State.Normal);
                }
                break;
            case State.Normal:
                if (fuelAmount <= 0f)
                {
                    return;
                }

                if (Keyboard.current.upArrowKey.isPressed ||
                   Keyboard.current.leftArrowKey.isPressed ||
                   Keyboard.current.rightArrowKey.isPressed)
                {
                    ConsumeFuel();
                }
                if (Keyboard.current.upArrowKey.isPressed)
                {
                    float force = 700f;
                    rd.AddForce(force * transform.up * Time.deltaTime);
                    onUpForce?.Invoke(this, EventArgs.Empty);
                }
                if (Keyboard.current.leftArrowKey.isPressed)
                {
                    float turnSpeed = +100f;
                    rd.AddTorque(turnSpeed * Time.deltaTime);
                    onLeftForce?.Invoke(this, EventArgs.Empty);

                }
                if (Keyboard.current.rightArrowKey.isPressed)
                {
                    float turnSpeed = -100f;
                    rd.AddTorque(turnSpeed * Time.deltaTime);
                    onRightForce?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.GameOver:
                break;
        }


       
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.TryGetComponent( out LandingPlace landingPlace))
        {
            Debug.Log("Crashed on the Terrain");
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
        float softLandingVelocityMagnitude = 4f;
        float relaticeVelocityMagnitude = collision.relativeVelocity.magnitude;
        if (relaticeVelocityMagnitude > softLandingVelocityMagnitude)
        {
            Debug.Log("Landed was high");
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
        float minDotVector = .90f;
        if(dotVector < minDotVector)
        {
            Debug.Log("Landed on a too steep angle");
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

        onLanded?.Invoke(this, new OnLandedEventArgs
        {
            landingType = LandingType.Success,
            dotVector = dotVector,
            speed = relaticeVelocityMagnitude,
            scoreMultiplier = landingPlace.GetScoreMultiplier(),
            score = score,
        }) ;
        SetState(State.GameOver);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out FuelPickUp fuel))
        {
            float addFuelAmount = 10f;
            fuelAmount += addFuelAmount;
            if (fuelAmount > fuelAmountMax)
            {
                fuelAmount = fuelAmountMax;
            }
            fuel.DestroyFuel();
        }

        if (collision.gameObject.TryGetComponent(out CoinPickUp coin))
        {
            onCoinPickUp?.Invoke(this, EventArgs.Empty);
            coin.DestroyCoin();
        }
    }


    private void SetState(State state)
    {
        this.state = state;
        onStateChange?.Invoke(this, new OnStateChangeEventArgs
        {
            ste = state
        }); ;
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
