using UnityEngine;

public class LowFuelUI : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        float fuel =
            PlayerMovement.instance.GetFuelAmountNormalized();

        if (fuel < 0.3f)
        {
            canvasGroup.alpha =
                0.5f + Mathf.PingPong(Time.time * 2f, 0.5f);
        }
        else
        {
            canvasGroup.alpha = 0f;
        }
    }
}