using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinPickupPopupPool : MonoBehaviour
{
    private const int DefaultInitialSize = 8;

    private static CoinPickupPopupPool instance;

    [SerializeField] private int initialSize = DefaultInitialSize;

    private readonly Queue<CoinPickupPopupUI> availablePopups = new Queue<CoinPickupPopupUI>();

    public static CoinPickupPopupPool GetOrCreateInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        instance = FindFirstObjectByType<CoinPickupPopupPool>();

        if (instance != null)
        {
            return instance;
        }

        GameObject poolObject = new GameObject("CoinPickupPopupPool");
        instance = poolObject.AddComponent<CoinPickupPopupPool>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        for (int i = 0; i < initialSize; i++)
        {
            CoinPickupPopupUI popup = CreatePopup();
            popup.gameObject.SetActive(false);
            availablePopups.Enqueue(popup);
        }
    }

    public CoinPickupPopupUI Get(Vector3 position)
    {
        CoinPickupPopupUI popup = availablePopups.Count > 0
            ? availablePopups.Dequeue()
            : CreatePopup();

        popup.transform.position = position;
        popup.gameObject.SetActive(true);
        return popup;
    }

    public void Release(CoinPickupPopupUI popup)
    {
        if (popup == null)
        {
            return;
        }

        popup.ResetPopup();
        popup.gameObject.SetActive(false);
        availablePopups.Enqueue(popup);
    }

    private CoinPickupPopupUI CreatePopup()
    {
        GameObject popupObject = new GameObject(
            "CoinPickupPopup",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasGroup),
            typeof(Image),
            typeof(Outline),
            typeof(CoinPickupPopupUI));

        popupObject.transform.SetParent(transform, false);

        Canvas canvas = popupObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 80;

        CoinPickupPopupUI popup = popupObject.GetComponent<CoinPickupPopupUI>();
        popup.SetPool(this);
        return popup;
    }
}
