using System.Collections;
using Unity.Cinemachine;
using UnityEngine;


public class CinemachineCameraShake : MonoBehaviour
{
    public static CinemachineCameraShake Instance { get; private set; }
    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        Instance = this;
        cinemachineBasicMultiChannelPerlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera(float intensity, float time)
    {
        if (cinemachineBasicMultiChannelPerlin == null)
        {
            Debug.LogWarning("CinemachineCameraShake needs a CinemachineBasicMultiChannelPerlin component.");
            return;
        }

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine(intensity, time));
    }

    private IEnumerator ShakeRoutine(float intensity, float time)
    {
        cinemachineBasicMultiChannelPerlin.AmplitudeGain = intensity;

        yield return new WaitForSecondsRealtime(time);

        ResetShake();
        shakeCoroutine = null;
    }

    private void OnDisable()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        ResetShake();
    }

    private void ResetShake()
    {
        if (cinemachineBasicMultiChannelPerlin != null)
        {
            cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0f;
        }
    }
}
