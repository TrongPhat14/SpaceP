using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    public void OnClickOpenSettings()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.PauseGame();
    }
}
