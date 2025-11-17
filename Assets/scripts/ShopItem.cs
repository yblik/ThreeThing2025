using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Shop/Create New Shop Item")]
public class ShopItem : ScriptableObject
{
    public string itemName;
    public int cost;

    [Header("Prefab To Spawn When Bought")]
    public GameObject itemPrefab;

    [Header("Optional Preview")]
    public GameObject previewPrefab;

    [Header("Spawn Position")]
    public Vector3 spawnOffset;

    [Header("Effects")]
    public AudioClip buySound;
}
