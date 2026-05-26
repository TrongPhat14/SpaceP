using UnityEngine;

[CreateAssetMenu(menuName = "Store/Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemId;
    public string itemName;

    [TextArea] public string description;


    [Header("Upgrade")]
    public UpgradeType upgradeType;
    public int maxLevel = 3;

    [Header("Price")]
    public int basePrice = 500;
    public int priceIncrease = 300;

    public int GetPrice(int currentLevel)
    {
        return basePrice + currentLevel * priceIncrease;
    }
}