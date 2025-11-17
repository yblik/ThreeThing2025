using UnityEngine;

public class PressurePlatePurchase : MonoBehaviour
{
    [Header("Purchase Settings")]
    public int cost = 50;
    public GameObject itemToUnlock;   // The item/building you want to enable
    public bool destroyAfterPurchase = true;
    public bool oneTimePurchase = true;

    [Header("Detection")]
    public string playerTag = "Player";
    public float activationDelay = 0.2f;

    private bool hasPurchased = false;
    private Bank bank;

    private void Start()
    {
        if (itemToUnlock != null)
            itemToUnlock.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (oneTimePurchase && hasPurchased) return;

        // find bank on player
        bank = other.GetComponentInParent<Bank>();
        if (bank == null)
        {
            Debug.LogError("No Bank component found on player!");
            return;
        }

        StartCoroutine(ProcessPurchase());
    }

    private System.Collections.IEnumerator ProcessPurchase()
    {
        yield return new WaitForSeconds(activationDelay);

        if (bank.Dinero >= cost)
        {
            // Deduct money
            bank.Dinero -= cost;
            bank.SaveMoney(); // saves PlayerPrefs

            // Unlock item
            if (itemToUnlock != null)
                itemToUnlock.SetActive(true);

            hasPurchased = true;

            if (destroyAfterPurchase)
                Destroy(gameObject);

            Debug.Log("Purchased for " + cost);
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }
}
