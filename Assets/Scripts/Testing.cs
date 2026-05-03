using UnityEngine;

public class Testing : MonoBehaviour
{

    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform pointC;
    private float interpolateAmount;
    [SerializeField] private float duration = 2f; 

    private void Update()
    {
        interpolateAmount += Time.deltaTime / duration;

        if (interpolateAmount > 1f)
        {
            interpolateAmount = 0f;
        }

        pointC.position = Vector3.Lerp(pointA.position, pointB.position, interpolateAmount);
    }


}
