using UnityEngine;

public class objectPlacer : MonoBehaviour
{
    [Header("References")]
    public Transform footPos;              // height reference
    public LayerMask surfaceMask;
    public float storerOffset = 0.2f;

    [Header("Placement Settings")]
    public float rotationSpeed = 90f;
    public float maxSlopeAngle = 5f;

    [Header("Auto Elevation Settings")]
    public float elevationStep = 0.02f;
    public float maxElevation = 1f;

    private bool placing = false;
    private GameObject previewInstance;
    private BoxCollider previewBounds;
    private float currentRotation = 0f;

    private InventoryItem selectedItem; // The selected item from inventory
    private InventorySlot selectedSlot; // The actual slot (to reduce amount)

    void Update()
    {
        // Toggle placement mode
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!placing) StartPlacementMode();
            else CancelPlacement();
        }

        if (placing)
        {
            UpdatePreviewPosition();
            HandleRotation();
            HandleInputConfirmCancel();
        }
    }

    // ---------------------------------------------------------------
    // SELECT ITEM FROM INVENTORY (CALLED BY UI OR SCROLL SYSTEM)
    // ---------------------------------------------------------------
    public void SelectInventoryItem(InventorySlot slot)
    {
        selectedSlot = slot;
        selectedItem = slot?.item;

        if (placing)
        {
            Destroy(previewInstance);
            StartPlacementMode();
        }
    }

    // ---------------------------------------------------------------
    // START / CANCEL
    // ---------------------------------------------------------------
    void StartPlacementMode()
    {
        if (selectedItem == null)
        {
            Debug.Log("No inventory item selected.");
            return;
        }

        if (selectedSlot.amount <= 0)
        {
            Debug.Log("Item out of stock.");
            return;
        }

        placing = true;

        // Build preview automatically
        previewInstance = Instantiate(selectedItem.worldPrefab);
        previewBounds = previewInstance.GetComponentInChildren<BoxCollider>();

        // Disable real physics for preview
        foreach (Collider c in previewInstance.GetComponentsInChildren<Collider>())
            c.enabled = false;

        // Assign ignore layers
        SetLayerRecursively(previewInstance, LayerMask.NameToLayer("Ignore Raycast"));

        TintPreview(Color.red);
    }

    void CancelPlacement()
    {
        placing = false;
        if (previewInstance != null)
            Destroy(previewInstance);
    }

    // ---------------------------------------------------------------
    // POSITIONING & VALIDATION
    // ---------------------------------------------------------------
    void UpdatePreviewPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 200f, surfaceMask))
            return;

        float slope = Vector3.Angle(hit.normal, Vector3.up);
        bool flatEnough = slope <= maxSlopeAngle;

        if (!flatEnough)
        {
            TintPreview(Color.red);
            return;
        }

        // Base placement height: player's foot height
        float footY = footPos.position.y;

        Vector3 pos = new Vector3(hit.point.x, footY, hit.point.z);

        // Optional extra offset
        if (selectedItem.itemName.ToLower().Contains("storer"))
            pos += Vector3.up * storerOffset;

        // Apply normal alignment + your rotation
        previewInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        previewInstance.transform.Rotate(Vector3.up, currentRotation, Space.Self);

        previewInstance.transform.position = pos;

        // Validate fit
        bool fits = CheckFit();
        TintPreview(fits ? Color.green : Color.red);
    }

    bool CheckFit()
    {
        Vector3 size = previewBounds.size;
        Vector3 center = previewInstance.transform.TransformPoint(previewBounds.center);

        return !Physics.CheckBox(
            center,
            size / 2f,
            previewInstance.transform.rotation,
            ~LayerMask.GetMask("Ignore Raycast")
        );
    }

    // ---------------------------------------------------------------
    // ROTATION
    // ---------------------------------------------------------------
    void HandleRotation()
    {
        if (Input.GetKey(KeyCode.Comma))
            currentRotation -= rotationSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Period))
            currentRotation += rotationSpeed * Time.deltaTime;
    }

    // ---------------------------------------------------------------
    // CONFIRM PLACEMENT
    // ---------------------------------------------------------------
    void HandleInputConfirmCancel()
    {
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (AttemptPlaceObject())
                CancelPlacement();
        }
    }

    bool AttemptPlaceObject()
    {
        if (!CheckFit())
            return false;

        // World object
        GameObject placed = Instantiate(
            selectedItem.worldPrefab,
            previewInstance.transform.position,
            previewInstance.transform.rotation
        );

        // Inventory reduces by 1
        InventoryManager.Instance.RemoveItem(selectedItem, 1);

        return true;
    }

    // ---------------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------------
    void TintPreview(Color c)
    {
        foreach (Renderer r in previewInstance.GetComponentsInChildren<Renderer>())
        {
            if (r.material.HasProperty("_Color"))
                r.material.color = c;
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.transform)
            SetLayerRecursively(t.gameObject, layer);
    }
}
