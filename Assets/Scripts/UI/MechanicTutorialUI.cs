using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MechanicTutorialUI : MonoBehaviour
{
    private const int MaxVisibleRows = 3;

    public static MechanicTutorialUI Instance { get; private set; }

    private RectTransform panelRect;
    private CanvasGroup panelCanvasGroup;
    private TextMeshProUGUI categoryText;
    private TextMeshProUGUI titleText;
    private ScrollRect scrollRect;
    private Button continueButton;

    private readonly List<GameObject> rows = new List<GameObject>();

    private MechanicTutorialData currentData;
    private bool isShowing;
    private bool controlsLocked;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (!FindReferences())
        {
            Debug.LogError(
                "MechanicTutorialUI hierarchy is incomplete. Keep the expected child names.",
                this
            );
            gameObject.SetActive(false);
            return;
        }

        continueButton.onClick.AddListener(CloseAndRemember);
        SetRowsVisible(false);
        gameObject.SetActive(false);
    }

    public bool TryShow(MechanicTutorialData data)
    {
        if (data == null || data.items == null || data.items.Count == 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(data.tutorialId))
        {
            Debug.LogWarning(
                $"Mechanic tutorial '{data.name}' needs a unique tutorialId.",
                data
            );
            return false;
        }

        if (PlayerPrefs.GetInt(data.GetPlayerPrefsKey(), 0) == 1)
        {
            return false;
        }

        currentData = data;
        BuildContent(data);
        SetPlayerControlLocked(true);
        isShowing = true;
        gameObject.SetActive(true);

        panelCanvasGroup.DOKill();
        panelRect.DOKill();
        panelCanvasGroup.alpha = 0f;
        panelRect.localScale = Vector3.one * 0.92f;
        panelCanvasGroup
            .DOFade(1f, 0.2f)
            .SetUpdate(true)
            .SetLink(gameObject);
        panelRect
            .DOScale(1f, 0.25f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .SetLink(gameObject);

        return true;
    }

    private bool FindReferences()
    {
        Transform panel = transform.Find("TutorialPanel");
        if (panel == null)
        {
            return false;
        }

        panelRect = panel as RectTransform;
        panelCanvasGroup = panel.GetComponent<CanvasGroup>();
        categoryText = panel
            .Find("NewHazardText")
            ?.GetComponent<TextMeshProUGUI>();
        titleText = panel
            .Find("TitleText")
            ?.GetComponent<TextMeshProUGUI>();

        Transform contentGroup = panel.Find("ContentGroup");
        scrollRect = contentGroup?.GetComponent<ScrollRect>();
        Transform rowContent = contentGroup?.Find("Viewport/Content");

        rows.Clear();
        for (int index = 1; index <= MaxVisibleRows; index++)
        {
            Transform row = rowContent?.Find($"Row_{index:00}");
            if (row != null)
            {
                rows.Add(row.gameObject);
            }
        }

        continueButton = panel
            .Find("ContinueButton")
            ?.GetComponent<Button>();

        return panelRect != null &&
            panelCanvasGroup != null &&
            categoryText != null &&
            titleText != null &&
            scrollRect != null &&
            rows.Count == MaxVisibleRows &&
            continueButton != null;
    }

    private void BuildContent(MechanicTutorialData data)
    {
        int itemCount = Mathf.Min(
            data.items.Count,
            rows.Count
        );

        categoryText.text = data.category;
        titleText.text = data.title;

        for (int index = 0; index < rows.Count; index++)
        {
            bool hasData = index < itemCount;
            GameObject row = rows[index];
            row.SetActive(hasData);

            if (hasData)
            {
                SetRowData(row, data.items[index]);
            }
        }

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    private static void SetRowData(
        GameObject row,
        MechanicTutorialData.MechanicItem item
    )
    {
        Image icon = row.transform
            .Find("MechanicIcon")
            .GetComponent<Image>();
        icon.sprite = item.icon;

        TextMeshProUGUI description = row.transform
            .Find("DescriptionText")
            .GetComponent<TextMeshProUGUI>();
        description.text =
            $"<b>{item.itemName}</b>\n{item.description}";
    }

    private void CloseAndRemember()
    {
        if (!isShowing)
        {
            return;
        }

        PlayerPrefs.SetInt(currentData.GetPlayerPrefsKey(), 1);
        PlayerPrefs.Save();
        isShowing = false;

        panelCanvasGroup.DOKill();
        panelRect.DOKill();
        panelCanvasGroup
            .DOFade(0f, 0.15f)
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                SetPlayerControlLocked(false);
                gameObject.SetActive(false);
            });
    }

    private void SetRowsVisible(bool visible)
    {
        foreach (GameObject row in rows)
        {
            if (row != null)
            {
                row.SetActive(visible);
            }
        }
    }

    private void SetPlayerControlLocked(bool isLocked)
    {
        if (controlsLocked == isLocked)
        {
            return;
        }

        controlsLocked = isLocked;
        PlayerMovement.Instance?.SetTutorialControlLocked(isLocked);
    }

    private void OnDisable()
    {
        panelCanvasGroup?.DOKill();
        panelRect?.DOKill();
        isShowing = false;
        SetPlayerControlLocked(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(CloseAndRemember);
        }

        SetPlayerControlLocked(false);
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Tutorial/Reset Mechanic Tutorials")]
    private static void ResetMechanicTutorials()
    {
        string[] assetGuids = UnityEditor.AssetDatabase.FindAssets(
            "t:MechanicTutorialData"
        );

        foreach (string assetGuid in assetGuids)
        {
            string assetPath =
                UnityEditor.AssetDatabase.GUIDToAssetPath(assetGuid);
            MechanicTutorialData data =
                UnityEditor.AssetDatabase.LoadAssetAtPath<
                    MechanicTutorialData
                >(assetPath);

            if (data != null &&
                !string.IsNullOrWhiteSpace(data.tutorialId))
            {
                PlayerPrefs.DeleteKey(data.GetPlayerPrefsKey());
            }
        }

        PlayerPrefs.Save();
        Debug.Log("Loaded mechanic tutorial progress reset.");
    }
#endif
}
