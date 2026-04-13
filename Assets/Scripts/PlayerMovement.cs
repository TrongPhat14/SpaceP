using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rd;

    private void Awake()
    {
        rd = GetComponent<Rigidbody2D>();
/*        Debug.Log(Vector2.Dot(new Vector2(0, 1), new Vector2(0, 1)));
        Debug.Log(Vector2.Dot(new Vector2(0, 1), new Vector2(.5f, .5f)));
        Debug.Log(Vector2.Dot(new Vector2(0, 1), new Vector2(1, 0)));
        Debug.Log(Vector2.Dot(new Vector2(0, 1), new Vector2(0, -1)));*/

    }

    private void FixedUpdate()
    {
       if(Keyboard.current.upArrowKey.isPressed)
        {
            float force = 700f;
            rd.AddForce(force * transform.up * Time.deltaTime);
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            float turnSpeed = +100f;
            rd.AddTorque(turnSpeed * Time.deltaTime);
        }
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            float turnSpeed = -100f;
            rd.AddTorque(turnSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.TryGetComponent( out LandingPlace landingPlace))
        {
            Debug.Log("Crashed on the Terrain");
            return;
        }
        float softLandingVelocityMagnitude = 4f;
        float relaticeVelocityMagnitude = collision.relativeVelocity.magnitude;
        if (relaticeVelocityMagnitude > softLandingVelocityMagnitude)
        {
            Debug.Log("Landed was high");
            return;
        }

        float dotVector = Vector2.Dot(Vector2.up, transform.up);
        float minDotVector = .90f;
        if(dotVector < minDotVector)
        {
            Debug.Log("Landed on a too steep angle");
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
    }
}
