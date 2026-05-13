using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI scoreTextMesh;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScreen);
        });
    }

    private void Start()
    {
        mainMenuButton.Select();
        scoreTextMesh.text = "Final score: " + GameManager.Instance.GetTotalScore().ToString();
    }

}
