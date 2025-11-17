using UnityEngine;

public class PressurePlateVisual : MonoBehaviour
{
    [Header("References")]
    public Bank playerBank;
    public MeshRenderer plateRenderer;
    public Transform plateTop; // object that visually moves down

    [Header("Plate Animation")]
    public float pressDistance = 0.05f;
    public float pressSpeed = 6f;
    private Vector3 originalTopPos;

    [Header("Colours")]
    public Color defaultColor = Color.yellow;
    public Color canAffordColor = Color.green;
    public Color cannotAffordColor = Color.red;

    [Header("Cost (Same as Purchase Script)")]
    public int cost = 50;

    [Header("Detection")]
    public string playerTag = "Player";
    private bool playerIsOnPlate = false;

    private void Start()
    {
        if (plateRenderer == null)
            plateRenderer = GetComponent<MeshRenderer>();

        originalTopPos = plateTop.localPosition;

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

        plateTop.localPosition = Vector3.Lerp(
            plateTop.localPosition,
            targetPos,
            Time.deltaTime * pressSpeed
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerIsOnPlate = true;

            // Assign bank reference if missing
            if (playerBank == null)
                playerBank = other.GetComponentInParent<Bank>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerIsOnPlate = false;
    }
}
