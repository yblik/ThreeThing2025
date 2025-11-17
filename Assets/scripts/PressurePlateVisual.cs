using UnityEngine;

public class PressurePlateVisual : MonoBehaviour
{
    [Header("References")]
    public Bank playerBank;
    public MeshRenderer plateRenderer;
    public Transform plateTop;
    private Transform objectToMove;
    private Renderer objectRenderer;

    [Header("Plate Animation")]
    public float pressDistance = 0.05f;
    public float pressSpeed = 6f;
    private Vector3 originalTopPos;

    [Header("Colours")]
    public Color defaultColor = Color.yellow;
    public Color canAffordColor = Color.green;
    public Color cannotAffordColor = Color.red;

    [Header("Cost")]
    public int cost = 50;

    [Header("Detection")]
    public string playerTag = "Player";
    private bool playerIsOnPlate = false;

    private void Start()
    {
        objectToMove = plateTop != null ? plateTop : transform;

        plateRenderer = plateRenderer != null ? plateRenderer : GetComponent<MeshRenderer>();
        objectRenderer = plateRenderer;

        originalTopPos = objectToMove.localPosition;

        plateRenderer.material.color = defaultColor;
    }


    private void Update()
    {
        if (playerBank != null)
            UpdateColour();

        AnimatePlate();
    }

    private void UpdateColour()
    {
        if (playerBank.Dinero >= cost)
            plateRenderer.material.color = canAffordColor;
        else
            plateRenderer.material.color = cannotAffordColor;
    }

    private void AnimatePlate()
    {
        Vector3 targetPos = originalTopPos;

        if (playerIsOnPlate)
            targetPos = originalTopPos - new Vector3(0, pressDistance, 0);

        objectToMove.localPosition = Vector3.Lerp(
            objectToMove.localPosition,
            targetPos,
            Time.deltaTime * pressSpeed
        );
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        Debug.Log("Player stepped on plate");

        playerIsOnPlate = true;

        if (playerBank == null)
        {
            playerBank = other.GetComponentInParent<Bank>();
            if (playerBank == null)
                Debug.LogError("No Bank found on player!", other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player left plate");
            playerIsOnPlate = false;
        }
    }
}
