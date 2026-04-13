using UnityEngine;

public class LandingPlace : MonoBehaviour
{
    [SerializeField] private int scoreMultiplier;

    public int GetScoreMultiplier()
    {
        return scoreMultiplier;
    }
}
