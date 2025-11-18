using UnityEngine;

public class SnakeCollectable : MonoBehaviour
{
    private SnakeHandler _handler;

    [Header("Audio")]
    public AudioClip catchSound;
    public float soundVolume = 1f;

    //constructor used by snake handler class when spawned in
    public void Initialize(SnakeHandler handler)
    {
        _handler = handler;
    }

    //following method is called when the snake is collected
    public void Collect()
    {//if statement doesn't work then just destroy the game object
            Destroy(gameObject);
    
    }
    //checks for collision if it's player
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
            PlaySound(catchSound);
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