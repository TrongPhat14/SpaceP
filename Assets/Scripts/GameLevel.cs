using UnityEngine;

public class GameLevel : MonoBehaviour
{

    [SerializeField] private int gameLevel;
    [SerializeField] private Transform landerStartPosition;

    public int GetLevelNumber()
    {
        return gameLevel;
    }

    public Vector3 GetLanderStartPosition()
    {
        return landerStartPosition.position;
    }

}
