using Unity.Cinemachine;
using UnityEngine;

public class CinemachineCameraZoom : MonoBehaviour
{
    public const float NORMAL_ORTHOGRAPHIC_SIZE = 10f;

    public static CinemachineCameraZoom Instance { get; private set; }
    [SerializeField] private CinemachineCamera cinemachineCamera;

    private float targetOrthographicSize = 10f;


    private void Awake()
    {
        Instance = this;
    }

    public void Update()
    {
        float zoomSpead = 2f;
        cinemachineCamera.Lens.OrthographicSize = 
            Mathf.Lerp(cinemachineCamera.Lens.OrthographicSize,targetOrthographicSize, Time.deltaTime * zoomSpead);
    }

    public void SetTargetOrthographicSize(float targetOrthographicSize)
    {
        this.targetOrthographicSize = targetOrthographicSize;
    }

    public void SetNormalOrthographicSize()
    {
        SetTargetOrthographicSize(NORMAL_ORTHOGRAPHIC_SIZE);
    }
}
