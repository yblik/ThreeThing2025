using UnityEngine;

public class PressurePlatePurchase : MonoBehaviour
{
    public int cost = 50;
    public GameObject itemToUnlock;
    public bool destroyAfterPurchase = true;
    public bool oneTimePurchase = true;

    public string playerTag = "Player";
    public float activationDelay = 0.2f;

    private bool hasPurchased = false;

    private void Start()
    {
        if (itemToUnlock != null)
            itemToUnlock.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (hasPurchased && oneTimePurchase) return;

        Debug.Log("Player triggered purchase plate.");

        Bank bank = other.GetComponentInParent<Bank>();

        if (bank == null)
        {
            Debug.LogError("Bank NOT found on player!", other);
            return;
        }

        StartCoroutine(ProcessPurchase(bank));
    }

    private System.Collections.IEnumerator ProcessPurchase(Bank bank)
    {
        yield return new WaitForSeconds(activationDelay);

        if (bank.Dinero >= cost)
        {
            Debug.Log("Purchase successful!");

            bank.Dinero -= cost;
            bank.SaveMoney();

            if (itemToUnlock != null)
                itemToUnlock.SetActive(true);

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
