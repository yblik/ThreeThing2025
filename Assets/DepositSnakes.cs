using UnityEngine;

public class DepositSnakes : MonoBehaviour
{
    //[Header("References")]
    private Transform player;
    private PlayerCatch PC;      // Reference to PlayerCatch script
    private Bank Natwest;         // Reference to your money system

    [Header("Settings")]
    public float triggerDistance = 3f;
    public int depositedSnakes = 0;
    public int maxDeposits = 20;

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
        if (distance <= triggerDistance && depositedSnakes < maxDeposits)
        {
            // Visual or sound cue could go here ("Press E to deposit snakes")

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (PC.StoredCollectibles > 0)
                {
                    Debug.Log($"Deposited {PC.StoredCollectibles} snake(s)!");

                    depositedSnakes += PC.StoredCollectibles;

                    // Call your bank to add the earnings
                    Natwest.WorkdaysWorking(PC.StoredCollectibles);

                    // Reset player’s inventory
                    PC.StoredCollectibles = 0;
                }
                else
                {
                    Debug.Log("No snakes to deposit!");
                }
            }
        }
    }
}
