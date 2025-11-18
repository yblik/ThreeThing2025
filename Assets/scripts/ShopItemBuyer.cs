using UnityEngine;

public class ShopItemBuyer : MonoBehaviour
{
    public ShopItem item;            // Assign your ShopItem asset here
    public Bank bank;                // Reference to Bank script
    public Inventory inventory;     // Where the item will appear

    public int type; //0 for storage, 1 for increasor, 2 for trap

    public void TryBuy()
    {
        if (bank.Dinero >= item.cost)
        {
            if (type == 0)
            {
                inventory.storage++;
            }
            if (type == 1)
            {
                inventory.increasers++;
            }
            if (type == 2)
            {
                inventory.traps++;
            }
            inventory.SaveInventory();

            bank.Dinero -= item.cost;
            bank.SaveMoney();

           
            // for inventory system, you might want to add the item to the player's inventory here
            //replace that instantiate line with inventory addition code here
            //InventoryManager.Instance.AddItem(shopItem.item, 1);


            Debug.Log(item.itemName + " purchased!");
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }
}
