using TMPro;
using UnityEngine;

public class LandedUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleTextMesh;
    [SerializeField] private TextMeshProUGUI statsTextMesh;

    private void Start()
    {
        PlayerMovement.instance.onLanded += lander_onLanded;

        Hide();

    }

    private void lander_onLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        if(e.landingType == PlayerMovement.LandingType.Success)
        {
            titleTextMesh.text = "SUCCESSFUL LANDING!";
        }
        else
        {
            titleTextMesh.text = "<color=#ff0000>CRASH!</color>";
        }

        statsTextMesh.text =
            Mathf.Round(e.speed * 2f) + "\n" +
            Mathf.Round(e.dotVector * 100f) + "\n" +
            "x" + e.scoreMultiplier + "\n" +
            e.score;

        Debug.Log(e.speed);
        Debug.Log(e.dotVector);
        Debug.Log(e.scoreMultiplier);
        Debug.Log(e.score);


        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    
    private void Hide()
    {
        gameObject.SetActive(false);

    }
}
