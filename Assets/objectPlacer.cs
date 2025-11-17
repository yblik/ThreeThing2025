using UnityEngine;

public class objectPlacer : MonoBehaviour
{
    public Transform footPos;
    [Header("References for storage")]
    public Transform player;
    public PlayerCatch PC;       // Reference to PlayerCatch script
    public Bank Natwest;
    public float storerOffset = 0.2f;

    [Header("Object List")]
    public GameObject[] placeablePrefabs;       // Real prefabs
    public GameObject[] previewPrefabs;         // Preview versions (transparent)
    public int selectedIndex = 0;               // Current selected object

    [Header("Placement Settings")]
    public float rotationSpeed = 90f;
    public LayerMask surfaceMask;
    public float maxSlopeAngle = 5f;            // Max tilt allowed for placement

    private bool placingMode = false;
    private GameObject previewInstance;
    private float currentRotation = 0f;
    private BoxCollider previewBounds;

    [Header("Auto Elevation Settings")]
    public float elevationStep = 0.02f;     // step upward each test
    public float maxElevation = 1f;         // max height allowed

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!placingMode) StartPlacement();
            else CancelPlacement();
        }

        if (placingMode)
        {
            HandlePlacement();
            HandleRotation();
            HandleConfirmCancel();
        }
    }

    void StartPlacement()
    {
        placingMode = true;

        previewInstance = Instantiate(previewPrefabs[selectedIndex]);
        previewBounds = previewInstance.GetComponentInChildren<BoxCollider>();

        // Disable preview colliders
        foreach (Collider c in previewInstance.GetComponentsInChildren<Collider>())
            c.enabled = false;

        SetLayerRecursively(previewInstance, LayerMask.NameToLayer("Ignore Raycast"));
    }

    void CancelPlacement()
    {
        placingMode = false;
        if (previewInstance != null)
            Destroy(previewInstance);
    }

    void HandlePlacement()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, surfaceMask))
        {
            float slope = Vector3.Angle(hit.normal, Vector3.up);
            bool isFlat = slope <= maxSlopeAngle;

            if (!isFlat)
            {
                TintPreview(Color.red);
                return;
            }

            // base position from raycast
            Vector3 basePos = hit.point;

            // snap Y to player's foot height
            float footY = footPos.position.y;

            // keep world XZ from hit point but raise Y to foot height
            Vector3 elevatedPos = new Vector3(basePos.x, footY, basePos.z);

            // extra offset if selected index is 0
            if (selectedIndex == 0)
                elevatedPos += Vector3.up * storerOffset;

            // apply rotation
            previewInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            previewInstance.transform.Rotate(Vector3.up, currentRotation, Space.Self);

            // move preview to elevated position
            previewInstance.transform.position = elevatedPos;

            // check fitting
            bool fits = CheckBoxFit();

            TintPreview(fits ? Color.green : Color.red);
        }
    }




    bool CheckBoxFit()
    {
        Vector3 size = previewBounds.size;
        Vector3 center = previewInstance.transform.TransformPoint(previewBounds.center);

        // BoxCast to see if object fits without hitting anything
        if (Physics.CheckBox(
            center,
            size / 2f,
            previewInstance.transform.rotation,
            ~LayerMask.GetMask("Ignore Raycast")      // ignore preview only
        ))
            return false;

        return true;
    }

    void HandleRotation()
    {
        if (Input.GetKey(KeyCode.Comma))
            currentRotation -= rotationSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Period))
            currentRotation += rotationSpeed * Time.deltaTime;
    }

    void HandleConfirmCancel()
    {
        // Place object
        if (Input.GetMouseButtonDown(0))
        {
            if (TryPlaceObject())
                CancelPlacement();
        }

        // Cancel with right-click
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    bool TryPlaceObject()
    {
        if (!CheckBoxFit())
            return false;

        GameObject obj = Instantiate(placeablePrefabs[selectedIndex]);

        if (selectedIndex == 0)
            obj.GetComponent<DepositSnakes>().DSconstructor(player, PC, Natwest);

        // final placement position = preview
        Vector3 pos = previewInstance.transform.position;

        obj.transform.position = pos;
        obj.transform.rotation = previewInstance.transform.rotation;

        return true;
    }


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
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    // Call externally: change selected object
    public void SetSelectedIndex(int index)
    {
        selectedIndex = index;

        if (placingMode)
        {
            Destroy(previewInstance);
            StartPlacement();   // restart with new preview
        }
    }
}
