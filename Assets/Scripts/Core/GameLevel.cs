using UnityEngine;

public class GameLevel : MonoBehaviour
{
    private const int MIN_LEVEL_NUMBER = 1;

    [SerializeField] private int gameLevel;
    [SerializeField] private Transform landerStartPosition;
    [SerializeField] private Transform cameraStartPosition;
    [SerializeField] private float zoomOutOrthographicSize;
    [SerializeField] private int completeCoinReward = 100;
    [SerializeField] private MechanicTutorialData mechanicTutorial;

    public int GetLevelNumber()
    {
        return gameLevel;
    }

    public bool HasRequiredReferences()
    {
        return landerStartPosition != null && cameraStartPosition != null;
    }

    public Vector3 GetLanderStartPosition()
    {
        return landerStartPosition != null ? landerStartPosition.position : transform.position;
    }

    public Transform GetCameraStartTargetTransform()
    {
        return cameraStartPosition != null ? cameraStartPosition : transform;
    }

    public float GetZoomOutOrthographicSize()
    {
        return zoomOutOrthographicSize;
    }

    public int GetCompleteCoinReward()
    {
        return completeCoinReward;
    }

    public MechanicTutorialData GetMechanicTutorial()
    {
        return mechanicTutorial;
    }

    private void OnValidate()
    {
        gameLevel = Mathf.Max(MIN_LEVEL_NUMBER, gameLevel);
        completeCoinReward = Mathf.Max(0, completeCoinReward);
        zoomOutOrthographicSize = Mathf.Max(0f, zoomOutOrthographicSize);
    }
}
