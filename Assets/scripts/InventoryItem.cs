using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Create Item")]
public class InventoryItem : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject worldPrefab; // for placing or dropping
    public bool stackable = true;
}

