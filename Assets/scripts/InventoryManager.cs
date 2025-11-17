using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public List<InventorySlot> inventory = new List<InventorySlot>();

    /// Add item to inventory
    public void AddItem(InventoryItem item, int amount = 1)
    {
        // If stackable, stack it
        if (item.stackable)
        {
            InventorySlot slot = inventory.Find(s => s.item == item);
            if (slot != null)
            {
                slot.amount += amount;
                return;
            }
        }

        // Otherwise create a new slot
        inventory.Add(new InventorySlot(item, amount));
    }

    /// Remove a certain amount
    public bool RemoveItem(InventoryItem item, int amount = 1)
    {
        InventorySlot slot = inventory.Find(s => s.item == item);

        if (slot == null) return false;

        if (slot.amount < amount) return false;

        slot.amount -= amount;

        if (slot.amount <= 0)
            inventory.Remove(slot);

        return true;
    }

    /// Check if you have enough of something
    public bool HasItem(InventoryItem item, int amount = 1)
    {
        InventorySlot slot = inventory.Find(s => s.item == item);
        return slot != null && slot.amount >= amount;
    }

    /// Get list of everything
    public List<InventorySlot> GetInventory()
    {
        return inventory;
    }
}
