using TMPro;
using UnityEngine;

[RequireComponent(typeof(LandingPlace))]
public class LandingPlaceVisual : MonoBehaviour
{
    [SerializeField] private TextMeshPro scoreMultiplierTextMesh;

    private LandingPlace landingPlace;

    private void Awake()
    {
        landingPlace = GetComponent<LandingPlace>();
    }

    private void Start()
    {
        RefreshMultiplierText();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            landingPlace = GetComponent<LandingPlace>();
        }

        RefreshMultiplierText();
    }

    private void RefreshMultiplierText()
    {
        if (scoreMultiplierTextMesh == null || landingPlace == null)
        {
            return;
        }

        scoreMultiplierTextMesh.text = $"x{landingPlace.GetScoreMultiplier()}";
    }
}
