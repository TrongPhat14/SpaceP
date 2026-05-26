using UnityEngine;
using UnityEngine.Rendering;

public class LowFuelVolume : MonoBehaviour
{
    private Volume lowFuelVolume;

    private void Awake()
    {
        lowFuelVolume = GetComponent<Volume>();
    }

    private void Update()
    {
        float fuel =
            PlayerMovement.Instance.GetFuelAmountNormalized();

        if (fuel < 0.3f)
        {
            float pulse =
                0.5f + Mathf.PingPong(Time.time * 2f, 0.5f);

            lowFuelVolume.weight = pulse;
        }
        else
        {
            lowFuelVolume.weight = 0f;
        }
    }
}

