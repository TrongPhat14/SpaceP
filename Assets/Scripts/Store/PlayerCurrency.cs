using UnityEngine;

public static class PlayerCurrency
{
    private static bool isLoaded;
    private static int coins;

    private const int MaxCoins = 5000;

    private static void EnsureLoaded()
    {
        if (isLoaded)
        {
            return;
        }

        SaveData saveData = SaveManager.LoadProgress();
        coins = saveData.coins;
        isLoaded = true;
    }

    public static int GetCoins()
    {
        EnsureLoaded();
        return coins;
    }

    public static void AddCoins(int amount)
    {
        EnsureLoaded();

        if (amount <= 0)
        {
            return;
        }

        coins = Mathf.Clamp(coins + amount, 0, MaxCoins);
        SaveManager.SaveCoins(coins);

        ReleaseLog.Log($"Add Coins: {amount}, Current Coins: {coins}");
    }


    // Use this when you want to change the color of an item but don't have enough money.
    public static bool HasEnoughCoins(int amount)
    {
        EnsureLoaded();
        return coins >= amount;
    }

    public static bool SpendCoins(int amount)
    {
        EnsureLoaded();

        if (amount <= 0)
        {
            return true;
        }

        if (coins < amount)
        {
            ReleaseLog.Log("Not enough coins.");
            return false;
        }

        coins -= amount;
        SaveManager.SaveCoins(coins);

        ReleaseLog.Log($"Spend Coins: {amount}, Current Coins: {coins}");
        return true;
    }

    // Use this when I want to reset the shop.
    public static void Reload()
    {
        isLoaded = false;
        EnsureLoaded();
    }
}
