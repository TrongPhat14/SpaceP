using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    public static SettingUI Instance { get; private set; }
    [SerializeField] private Button settingButton;

    private void Awake()
    {
        Instance = this;
        Show();
        settingButton.onClick.AddListener(() =>
        {
            GameManager.instance.PauseGame();
            Hide();
        });
    }


    public void Show()
    {
        gameObject.SetActive(true);
    }
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
