using UnityEngine;

public class WindForce : MonoBehaviour
{
    [SerializeField] private Vector2 direction = Vector2.right;
    [SerializeField] private float strength = 5f;

    public Vector2 GetDirection()
    {
        return direction.normalized;
    }

    public float GetStrength()
    {
        return strength;
    }
}