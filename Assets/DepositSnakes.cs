using UnityEngine;

public class DepositSnakes : MonoBehaviour
{
    //[Header("References")]
    public Transform player;
    public PlayerCatch PC;      // Reference to PlayerCatch script
    public Bank Natwest;         // Reference to your money system

    [Header("Settings")]
    public float triggerDistance = 3f;
    public int depositedSnakes = 0;
    public int maxDeposits = 20;

    public AudioSource DepositSFX;
    public AudioSource cantSFX;

    public void DSconstructor(Transform p, PlayerCatch pc, Bank natwest)
    {
        player = p;
        PC = pc;
        Natwest = natwest;
    }
    void Update()
    {
        if (player == null || PC == null || Natwest == null)
        {
            Debug.LogWarning("DepositSnakes: Missing required reference(s).");
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);

        // Only allow deposit if player is close and under limit
        if (distance <= triggerDistance)
        {
            // Visual or sound cue could go here ("Press E to deposit snakes")

            if (Input.GetKeyDown(KeyCode.E) && depositedSnakes < maxDeposits)
            {
                if (PC.StoredCollectibles > 0)
                {
                    Debug.Log($"Deposited {PC.StoredCollectibles} snake(s)!");

                    depositedSnakes += PC.StoredCollectibles;
                    DepositSFX.Play();

                    // Call your bank to add the earnings
                    Natwest.WorkdaysWorking(PC.StoredCollectibles);

                    // Reset player’s inventory
                    PC.StoredCollectibles = 0;
                }
                else
                {
                    cantSFX.Play();
                }
            }
        }
    }
}
