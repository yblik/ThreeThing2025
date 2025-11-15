using UnityEngine;

public class SCameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public bool playOnStart = false;
    public float shakeDuration = 1f;
    public float shakeMagnitude = 0.05f;
    public float shakeFrequency = 20f;

    private Vector3 originalPos;
    private float shakeTimer;

    void Start()
    {
        originalPos = transform.localPosition;

        if (playOnStart)
            StartShake(shakeDuration, shakeMagnitude);
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            float offsetX = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) * 2 - 1) * shakeMagnitude;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) * 2 - 1) * shakeMagnitude;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            shakeTimer -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }

    // Call this from UI Button, Timeline, Animation Event, etc.
    public void StartShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        shakeTimer = duration;
    }
}
