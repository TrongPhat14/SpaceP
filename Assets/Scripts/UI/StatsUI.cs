using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Image fuelImage;

    private float displayedFuelAmount = -1f;

    private void Start()
    {
        if (PlayerMovement.Instance == null)
        {
            return;
        }

        PlayerMovement.Instance.onFuelPickUp += Player_OnFuelPickUp;
        PlayerMovement.Instance.onCoinPickUp += Player_OnCoinPickUp;
    }

    private void Update()
    {
        UpdateStatsTextMesh();
    }

    private void UpdateStatsTextMesh()
    {
        float fuelAmount = PlayerMovement.Instance.GetFuelAmountNormalized();

        if (displayedFuelAmount < 0f)
        {
            displayedFuelAmount = fuelAmount;
            fuelImage.fillAmount = fuelAmount;
        }
        else if (Mathf.Abs(displayedFuelAmount - fuelAmount) > 0.005f)
        {
            displayedFuelAmount = fuelAmount;
            fuelImage.DOKill();
            fuelImage
                .DOFillAmount(fuelAmount, 0.12f)
                .SetLink(fuelImage.gameObject)
                .SetEase(Ease.OutQuad);
        }

        statsText.text =
            GameManager.Instance.GetLevelNumber() + "\n" +
            GameManager.Instance.GetScore() + "\n" +
            Mathf.Round(GameManager.Instance.GetTime())
            ;
    }

    private void Player_OnFuelPickUp(object sender, EventArgs e)
    {
        DOTweenUIAnimator.PunchScale(fuelImage != null ? fuelImage.transform : null, 0.12f);
    }

    private void Player_OnCoinPickUp(object sender, EventArgs e)
    {
        DOTweenUIAnimator.PunchScale(statsText != null ? statsText.transform : null, 0.08f);
    }

    private void OnDestroy()
    {
        if (PlayerMovement.Instance == null)
        {
            return;
        }

        PlayerMovement.Instance.onFuelPickUp -= Player_OnFuelPickUp;
        PlayerMovement.Instance.onCoinPickUp -= Player_OnCoinPickUp;
    }
}
