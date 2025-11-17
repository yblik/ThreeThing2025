using UnityEngine;

public class ShopItemBuyer : MonoBehaviour
{
    public ShopItem item;            // Assign your ShopItem asset here
    public Bank bank;                // Reference to Bank script
    public Transform spawnPoint;     // Where the item will appear

    public void TryBuy()
    {
        if (bank.Dinero >= item.cost)
        {
            bank.Dinero -= item.cost;
            bank.SaveMoney();

            // Spawn
            if (item.itemPrefab != null)
            {
                Instantiate(item.itemPrefab, spawnPoint.position + item.spawnOffset, Quaternion.identity);
            }

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
