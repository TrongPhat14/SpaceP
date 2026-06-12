using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MechanicTutorial",
    menuName = "Tutorial/Mechanic Tutorial Data"
)]
public class MechanicTutorialData : ScriptableObject
{
    public const int MaxItems = 3;

    [Serializable]
    public class MechanicItem
    {
        public string itemName;
        public Sprite icon;

        [TextArea(2, 5)]
        public string description;
    }

    [Header("Progress")]
    [Tooltip("Stable unique ID used to remember that this tutorial was viewed.")]
    public string tutorialId;

    [Header("Header")]
    public string category = "NEW HAZARD";
    public string title;

    [Header("Content")]
    [Tooltip("Supports one to three items. Multiple items are displayed as a sequence.")]
    public List<MechanicItem> items = new List<MechanicItem>();

    public string GetPlayerPrefsKey()
    {
        return SaveKeys.MechanicTutorialPrefix + tutorialId;
    }

    private void OnValidate()
    {
        if (items.Count > MaxItems)
        {
            items.RemoveRange(MaxItems, items.Count - MaxItems);
        }
    }
}
