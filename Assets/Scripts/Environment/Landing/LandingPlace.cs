using UnityEngine;

public class LandingPlace : MonoBehaviour
{
    [SerializeField, Min(1)] private int scoreMultiplier = 1;

    public int GetScoreMultiplier()
    {
        return Mathf.Max(1, scoreMultiplier);
    }

    private void OnValidate()
    {
        scoreMultiplier = Mathf.Max(1, scoreMultiplier);
    }
}
