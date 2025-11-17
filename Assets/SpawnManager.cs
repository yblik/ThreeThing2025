using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private int spawnIndex = 0; // Room index
    [SerializeField] private int posIndex = 0;   // Position index
    [SerializeField] private bool spawn = false; // True = respawn mode

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate SpawnManager detected. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetPoint(1); // Default to bed on game session
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Room spawn index
    public void SetSpawnPoint(int index) => spawnIndex = index;
    public int GetSpawnPoint() => spawnIndex;

    // Position index
    public void SetPoint(int index) => posIndex = index;
    public int GetPoint() => posIndex;

    // Respawn flag
    public void SetRespawn(bool value) => spawn = value;
    public bool GetRespawnPoint() => spawn;
}