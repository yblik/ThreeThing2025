using UnityEngine;

public class SnakeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.4f;       // how close to bite
    public float attackCooldown = 1.2f;    // cooldown between bites
    public float damage = 1f;              // damage dealt to player

    [Header("Raycast Options")]
    public float rayRadius = 0.25f;        // hit forgiveness, small spherecast

    [Header("References")]
    public Transform head;                 // snake head/mouth position
    public Transform player;               // PLAYER transform (no LayerMask now)

    private Health playerHealth;
    private float attackTimer;

    private void Start()
    {
        // Auto-assign player transform if not set
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // Get the player's health script
        if (player != null)
            playerHealth = player.GetComponent<Health>();
    }

    private void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
            TryAttackPlayer();
    }

    void TryAttackPlayer()
    {
        if (player == null || playerHealth == null || head == null)
            return;

        // Distance check first
        float dist = Vector3.Distance(head.position, player.position);
        if (dist > attackRange)
            return;

        // Spherecast toward the player
        Vector3 direction = (player.position - head.position).normalized;

        Debug.DrawRay(head.position, direction * attackRange, Color.red);

        RaycastHit hit;
        if (Physics.SphereCast(head.position, rayRadius, direction, out hit, attackRange))
        {
            if (hit.transform == player)
            {
                // Deal damage
                playerHealth.TakeDamage(damage);
                attackTimer = 0f;

                Debug.Log("Snake bit the player!");
            }
        }
    }
}
