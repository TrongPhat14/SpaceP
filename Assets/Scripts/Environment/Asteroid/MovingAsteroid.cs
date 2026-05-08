using UnityEngine;

public class MovingAsteroid : MonoBehaviour
{

    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform Asteroid;
    [SerializeField] private float duration = 2f;


    private float interpolateAmount;

    private void Update()
    {
        interpolateAmount += Time.deltaTime / duration;

        if (interpolateAmount > 1f)
        {
            interpolateAmount = 0f;
        }

        Asteroid.position = Vector3.Lerp(pointA.position, pointB.position, interpolateAmount);
    }

}
