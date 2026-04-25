using TMPro;
using UnityEngine;

public class LandingPlaceVisual : MonoBehaviour
{
    [SerializeField] private TextMeshPro scoreMultiplierTextMesh;

    private void Awake()
    {
        LandingPlace landingPlace = GetComponent<LandingPlace>();
        scoreMultiplierTextMesh.text = "x" + landingPlace.GetScoreMultiplier();
    }

}
