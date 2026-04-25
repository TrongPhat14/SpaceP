using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Image fuelImage;


    private void Update()
    {
        UpdateStatsTextMesh();
    }

    private void UpdateStatsTextMesh()
    {
        fuelImage.fillAmount = PlayerMovement.instance.GetFuelAmountNormalized();
        statsText.text =
            GameManager.instance.GetScore() + "\n" + "\n" +
            Mathf.Round(GameManager.instance.GetTime())
            ;
    }

}
