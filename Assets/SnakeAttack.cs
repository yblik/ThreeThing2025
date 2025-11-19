using UnityEngine;

public class SnakeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 1.4f;       // how close to bite
    public float attackCooldown = 1.2f;    // cooldown between bites
    public float damage = 1f;              // damage dealt to player

    [Header("Field of View Attack")]
    public float attackFOV = 45f;          // Only attack if player within 45° in front
    public bool requireDirectLineOfSight = true;
    public LayerMask obstructionMask = ~0;

    [Header("Raycast Options")]
    public float rayRadius = 0.25f;        // hit forgiveness, small spherecast

    [Header("References")]
    public Transform head;                 // snake head/mouth position
    public Transform player;               // PLAYER transform

    [Header("Audio")]
    public AudioClip ouchhitClip;
    public AudioClip biteClip;
    public float soundVolume = 1f;

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

        // FOV CHECK - Only attack if player is in front
        if (!IsPlayerInAttackFOV())
            return;

        // Line of sight check (optional)
        if (requireDirectLineOfSight && !HasLineOfSightToPlayer())
            return;

        // Distance check
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

                // Play sounds
                PlaySound(biteClip);
                PlaySound(ouchhitClip);

                Debug.Log("Snake bit the player!");
            }
        }
    }

    // FOV CHECK METHOD
    bool IsPlayerInAttackFOV()
    {
        if (player == null) return false;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0; // Ignore height differences

        float angle = Vector3.Angle(transform.forward, toPlayer);
        return angle <= attackFOV;
    }

    // LINE OF SIGHT CHECK METHOD
    bool HasLineOfSightToPlayer()
    {
        if (player == null || head == null) return false;

        Vector3 eyePosition = head.position;
        Vector3 playerCenter = player.position + Vector3.up * 0.5f; // Player center

        if (Physics.Linecast(eyePosition, playerCenter, out RaycastHit hit, obstructionMask))
        {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }

        return true;
    }

    // CALL THIS FROM AIControllerScript TO CHECK IF SHOULD ATTACK
    public bool ShouldAttack()
    {
        if (player == null || head == null) return false;

        // Check all conditions: FOV, line of sight, distance, cooldown
        return IsPlayerInAttackFOV() &&
               (!requireDirectLineOfSight || HasLineOfSightToPlayer()) &&
               Vector3.Distance(head.position, player.position) <= attackRange &&
               attackTimer >= attackCooldown;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        GameObject audioObj = new GameObject("Audio");
        AudioSource source = audioObj.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = soundVolume;
        source.spatialBlend = 0f; // 2D sound
        source.Play();

        Destroy(audioObj, clip.length);
    }

    void OnDrawGizmosSelected()
    {
        if (player == null || head == null) return;

        // Draw attack range
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(head.position, attackRange);

        // Draw FOV cone from snake body (not head)
        Gizmos.color = IsPlayerInAttackFOV() ? Color.red : Color.yellow;
        Vector3 leftBound = Quaternion.Euler(0, -attackFOV, 0) * transform.forward * attackRange;
        Vector3 rightBound = Quaternion.Euler(0, attackFOV, 0) * transform.forward * attackRange;

        Gizmos.DrawRay(transform.position, transform.forward * attackRange);
        Gizmos.DrawRay(transform.position, leftBound);
        Gizmos.DrawRay(transform.position, rightBound);

        // Draw line of sight from head to player
        if (requireDirectLineOfSight)
        {
            Gizmos.color = HasLineOfSightToPlayer() ? Color.green : Color.magenta;
            Gizmos.DrawLine(head.position, player.position + Vector3.up * 0.5f);
        }
    }
}