using Unity.Cinemachine;
using UnityEngine;


public class CinemachineCameraShake : MonoBehaviour
{
    public static CinemachineCameraShake Instance { get; private set; }
    private CinemachineCamera CinemachineVirtualCamera;
    private float shakeTimer;
    private void Awake()
    {
        Instance = this;
        CinemachineVirtualCamera = GetComponent<CinemachineCamera>();
    }

    public void ShakeCamera(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
        CinemachineVirtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();

        cinemachineBasicMultiChannelPerlin.AmplitudeGain = intensity;
        shakeTimer = time;
    }

    private void Update()
    {
        if(shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if(shakeTimer <= 0f ) 
            {
                CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
                    CinemachineVirtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
                cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0f;
            }
        }
    }
}
