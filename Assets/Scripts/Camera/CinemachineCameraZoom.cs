using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

public class CinemachineCameraZoom : MonoBehaviour
{
    public const float NORMAL_ORTHOGRAPHIC_SIZE = 10f;

    public static CinemachineCameraZoom Instance { get; private set; }
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float zoomDuration = 0.35f;

    private float targetOrthographicSize = 10f;
    private Tween zoomTween;


    private void Awake()
    {
        Instance = this;
    }
    public void SetTargetOrthographicSize(float targetOrthographicSize)
    {
        this.targetOrthographicSize = targetOrthographicSize;

        if (cinemachineCamera == null)
        {
            return;
        }

        zoomTween?.Kill();
        zoomTween = DOTween
            .To(
                () => cinemachineCamera.Lens.OrthographicSize,
                SetOrthographicSize,
                targetOrthographicSize,
                zoomDuration
            )
            .SetEase(Ease.OutSine);
    }

    public void SetNormalOrthographicSize()
    {
        SetTargetOrthographicSize(NORMAL_ORTHOGRAPHIC_SIZE);
    }

    private void SetOrthographicSize(float orthographicSize)
    {
        LensSettings lensSettings = cinemachineCamera.Lens;
        lensSettings.OrthographicSize = orthographicSize;
        cinemachineCamera.Lens = lensSettings;
    }

    private void OnDisable()
    {
        zoomTween?.Kill();
    }
}
