using JetBrains.Annotations;
using UnityEngine;

public class PressurePlatePurchase : MonoBehaviour
{
    public int cost = 50;
    //public GameObject itemToUnlock;
    public bool destroyAfterPurchase = true;
    public bool oneTimePurchase = true;

    public string playerTag = "Player";
    public float activationDelay = 0.2f;

    public Inventory inventory;

    public int Type;

    private bool hasPurchased = false;
    public Bank b;

    //private void Start()
    //{
    //    if (itemToUnlock != null)
    //        itemToUnlock.SetActive(false);
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (hasPurchased && oneTimePurchase) return;

        Debug.Log("Player triggered purchase plate.");


        if (b == null)
        {
            Debug.LogError("Bank NOT found on player!", other);
            return;
        }

        StartCoroutine(ProcessPurchase());
    }

    private System.Collections.IEnumerator ProcessPurchase()
    {
        yield return new WaitForSeconds(activationDelay);

        if (b.Dinero >= cost)
        {
            Debug.Log("Purchase successful!");

            b.Dinero -= cost;
            switch (Type)
            {
                case 0:
                    inventory.storage++;
                    break;
                case 1:
                    inventory.increasers++;
                    break;
                case 2:
                    inventory.traps++;
                    break;
            }

            inventory.SaveInventory();

            //if (itemToUnlock != null)
            //    itemToUnlock.SetActive(true);

            hasPurchased = true;

            if (destroyAfterPurchase)
                Destroy(gameObject);
        }
        else
        {
            Debug.Log("Purchase failed — not enough money.");
        }
    }
}
