using UnityEngine;

public class PlayerAnimManager : MonoBehaviour
{
    public Animator animator;
    public Rigidbody rb;

    [Header("Audio")]
    public AudioClip jumpClip;
    public AudioClip walkClip;
    public float soundVolume = 1f;
    public float walkVolume = 0.5f;
    public float walkFadeSpeed = 2f;

    private AudioSource walkAudioSource;   // For looping walking sound
    private AudioSource sfxAudioSource;    // For jump/hit one-shots
    private float targetVolume = 0f;

    private void Start()
    {
        // Walking AudioSource
        walkAudioSource = gameObject.AddComponent<AudioSource>();
        walkAudioSource.clip = walkClip;
        walkAudioSource.loop = true;
        walkAudioSource.volume = 0f; // start muted
        walkAudioSource.spatialBlend = 0f;
        walkAudioSource.Play();

        // SFX AudioSource for jump & hit
        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.loop = false;
        sfxAudioSource.spatialBlend = 0f;
    }

    private void Update()
    {
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        // Walking animation & sound fade
        animator.SetBool("w", isMoving);
        targetVolume = isMoving ? walkVolume : 0f;
        walkAudioSource.volume = Mathf.MoveTowards(walkAudioSource.volume, targetVolume, walkFadeSpeed * Time.deltaTime);

        // Jump animation & sound
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetBool("j", true);
            if (jumpClip != null)
                sfxAudioSource.PlayOneShot(jumpClip, soundVolume);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            animator.SetBool("j", false);
        }

        // Attack animation & sound
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetBool("s", true);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            animator.SetBool("s", false);
        }
    }
}
