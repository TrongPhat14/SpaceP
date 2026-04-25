using UnityEngine;

public class GameLevel : MonoBehaviour
{

    [SerializeField] private int gameLevel;
    [SerializeField] private Transform landerStartPosition;
    [SerializeField] private Transform cameraStartPosition;
    [SerializeField] private float zoomOutOrthographicSize;



    public int GetLevelNumber()
    {
        return gameLevel;
    }

    public Vector3 GetLanderStartPosition()
    {
        return landerStartPosition.position;
    }
    public Transform GetCameraStartTargetTransform()
    {
        return cameraStartPosition;
    }

    public float GetZoomOutOrthographicSize()
    {
        return zoomOutOrthographicSize;
    }
}
