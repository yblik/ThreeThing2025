using UnityEngine;

public class PressurePlatePurchase : MonoBehaviour
{
    [Header("Purchase Settings")]
    public int cost = 50;
    public bool destroyAfterPurchase = true;
    public bool oneTimePurchase = true;

    [Header("References")]
    public Inventory inventory;
    public Bank b;                     // Assign in Inspector
    public string playerTag = "Player";

    [Header("Timing")]
    public float activationDelay = 0.2f;

    [Header("Item Type")]
    public int Type; // 0 = storage, 1 = increasers, 2 = traps

    [Header("Audio")]
    public AudioClip purchaseSound;
    public AudioClip failedSound;
    public float soundVolume = 1f;

    private bool hasPurchased = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger by: {other.name}");

        if (!other.CompareTag(playerTag))
        {
            Debug.Log("Wrong tag.");
            return;
        }

        if (hasPurchased && oneTimePurchase)
            return;

        if (b == null)
        {
            Debug.LogError("Bank reference missing in Inspector!");
            return;
        }

        StartCoroutine(ProcessPurchase());
    }

    private System.Collections.IEnumerator ProcessPurchase()
    {
        yield return new WaitForSeconds(activationDelay);
        Debug.Log("Processing purchase...");

        if (b.Dinero >= cost)
        {
            Debug.Log("Purchase success!");

            b.Dinero -= cost;

            // Update inventory
            switch (Type)
            {
                case 0: inventory.storage++; break;
                case 1: inventory.increasers++; break;
                case 2: inventory.traps++; break;
            }

            inventory.SaveInventory();
            hasPurchased = true;

            PlaySound(purchaseSound);

            if (destroyAfterPurchase)
                Destroy(gameObject);
        }
        else
        {
            Debug.Log("Purchase failed — not enough money.");
            PlaySound(failedSound);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        // Create a temporary audio object
        GameObject audioObj = new GameObject("PurchaseSound");
        AudioSource source = audioObj.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = soundVolume;
        source.spatialBlend = 0f;           // 2D sound (change to 1f for 3D)
        source.Play();

        // Destroy after playback ends
        Destroy(audioObj, clip.length);
    }
}
