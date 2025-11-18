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

    [Header("Inventory Reference")]
    public Inventory inventory; // Assign your Inventory component here

    [Header("Item Prefabs")]
    public GameObject storagePrefab;    // For key 1
    public GameObject increaserPrefab;  // For key 2  
    public GameObject trapPrefab;       // For key 3

    private bool placing = false;
    private GameObject previewInstance;
    private BoxCollider previewBounds;
    private float currentRotation = 0f;

    // Track which item type we're placing (0=storage, 1=increaser, 2=trap)
    private int currentItemType = -1;
    private GameObject currentPrefab;

    bool HasAnyInventoryItems()
    {
        return inventory.storage > 0 || inventory.increasers > 0 || inventory.traps > 0;
    }
    void Update()
    {
        // Keyboard number key selection
        HandleKeyboardSelection();

        // Toggle placement mode with Q (existing functionality)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!placing && HasAnyInventoryItems()) StartPlacementMode();
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
    // KEYBOARD NUMBER KEY SELECTION (NEW)
    // ---------------------------------------------------------------
    void HandleKeyboardSelection()
    {
        // Key 1 - Storage
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (inventory.storage > 0)
            {
                currentItemType = 0;
                currentPrefab = storagePrefab;
                if (placing) CancelPlacement();
                StartPlacementMode();
            }
            else
            {
                Debug.Log("No storage items available!");
                if (placing) CancelPlacement();
            }
        }

        // Key 2 - Increasers
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (inventory.increasers > 0)
            {
                currentItemType = 1;
                currentPrefab = increaserPrefab;
                if (placing) CancelPlacement();
                StartPlacementMode();
            }
            else
            {
                Debug.Log("No increaser items available!");
                if (placing) CancelPlacement();
            }
        }

        // Key 3 - Traps
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (inventory.traps > 0)
            {
                currentItemType = 2;
                currentPrefab = trapPrefab;
                if (placing) CancelPlacement();
                StartPlacementMode();
            }
            else
            {
                Debug.Log("No trap items available!");
                if (placing) CancelPlacement();
            }
        }
    }

    // ---------------------------------------------------------------
    // START / CANCEL
    // ---------------------------------------------------------------
    void StartPlacementMode()
    {
        if (currentPrefab == null)
        {
            Debug.Log("No item prefab selected.");
            return;
        }

        // Check if player has the item
        if (!HasItemAvailable())
        {
            Debug.Log("Item out of stock.");
            return;
        }

        placing = true;

        // Build preview automatically
        previewInstance = Instantiate(currentPrefab);
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
        currentItemType = -1;
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
        if (currentItemType == 0) // Storage
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
            currentPrefab,
            previewInstance.transform.position,
            previewInstance.transform.rotation
        );

        // Reduce inventory count
        ReduceInventoryCount();

        return true;
    }

    // ---------------------------------------------------------------
    // INVENTORY HELPERS
    // ---------------------------------------------------------------
    bool HasItemAvailable()
    {
        switch (currentItemType)
        {
            case 0: return inventory.storage > 0;
            case 1: return inventory.increasers > 0;
            case 2: return inventory.traps > 0;
            default: return false;
        }
    }

    void ReduceInventoryCount()
    {
        switch (currentItemType)
        {
            case 0:
                inventory.storage--;
                break;
            case 1:
                inventory.increasers--;
                break;
            case 2:
                inventory.traps--;
                break;
        }
        inventory.SaveInventory();
        Debug.Log($"Placed item. Remaining - Storage: {inventory.storage}, Increasers: {inventory.increasers}, Traps: {inventory.traps}");
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