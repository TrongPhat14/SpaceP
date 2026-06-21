using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    private Button settingsButton;

    private void Awake()
    {
        settingsButton = GetComponent<Button>();
    }

    private void Start()
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.onLanded += PlayerMovement_OnLanded;
        }
    }

    public void OnClickOpenSettings()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.HasLevelEnded())
        {
            return;
        }

        GameManager.Instance.PauseGame();
    }

    private void PlayerMovement_OnLanded(object sender, PlayerMovement.OnLandedEventArgs e)
    {
        SetSettingsInteractable(false);
    }

    private void SetSettingsInteractable(bool interactable)
    {
        if (settingsButton != null)
        {
            settingsButton.interactable = interactable;
        }
    }

    private void OnDestroy()
    {
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.onLanded -= PlayerMovement_OnLanded;
        }
    }
}
