using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridPlacer : MonoBehaviour
{
    [Header("Setup")]
    public Camera mainCamera;
    public GameObject[] brushPrefabs;
    public Material previewMaterial; // Assign in Inspector
    public float cellSize = 1f;
    public LayerMask placementMask;

    [Header("UI things")]
    public GetUIButtonIcon uiButtons; // Reference to the button icon script

    [Header("Grid Settings")]
    public int gridWidth = 20;
    public int gridHeight = 20;
    public float layerHeight = 2f; // Height difference between elevation layers

    [Header("Placement Plane")]
    public Transform gridOrigin; // grid pivot for placement
    [Range(0f, 360f)]

    //[Header("Layering")]
    //public int currentElevation = 0; // Current active elevation for UI and placement
    [Header("Angling")]
    public float placementAngle = 0f; // rotation of placement plane
    private Quaternion placementRotation => Quaternion.Euler(0, placementAngle, 0);

    private int selectedBrushIndex = 0;
    private float currentRotation = 0f; // local object rotation
    private bool eraserMode = false;
    private float elevation = 0; //float for floor offset
    private int brushSize = 1;

    private bool brushActive = false;

    // === Position management ===
    private HashSet<Vector3> filledPositions = new HashSet<Vector3>();
    private Dictionary<Vector3, GameObject> placedObjects = new Dictionary<Vector3, GameObject>();



    private GameObject selectedObject = null;
    private Vector3 selectedGridPos;

    // === Preview object variables ===
    private GameObject previewObject;
    private Vector3 lastPreviewPos;
    private bool editMode = false; // true when moving a selected object
    private List<GameObject> previewObjects = new List<GameObject>();

#if UNITY_EDITOR
    private bool waitingForPreviews = false;
#endif

    void Start()
    {
        BuildBrushUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectBrush();
            return;
        }

        HandleCameraMovement();

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        HandleRotationInput();
        HandleLayerInput();

        HandlePreview();
        HandlePlacement();
        HandleSelection();
        HandleSelectedMovement();
    }

    // === UI SETUP ===
    public void BuildBrushUI()
    {
        if (uiButtons == null || uiButtons.buttonIcon.Length == 0 || brushPrefabs == null)
        {
            Debug.LogWarning("Missing UI buttons or prefabs!");
            return;
        }

        int count = Mathf.Min(uiButtons.buttonIcon.Length, brushPrefabs.Length);

        for (int i = 0; i < count; i++)
        {
            var prefab = brushPrefabs[i];
            var button = uiButtons.buttonIcon[i];
            if (prefab == null || button == null) continue;

            Image img = button.GetComponent<Image>();
            if (img == null) continue;

#if UNITY_EDITOR
            Texture2D preview = AssetPreview.GetAssetPreview(prefab);
            if (preview == null)
            {
                preview = AssetPreview.GetMiniThumbnail(prefab);
                waitingForPreviews = true;
            }

            if (preview != null)
            {
                img.sprite = Sprite.Create(preview, new Rect(0, 0, preview.width, preview.height), new Vector2(0.5f, 0.5f));
                img.color = Color.white;
            }
            else
            {
                img.color = Color.green;
                img.sprite = null;
            }
#else
            img.color = Color.gray;
            img.sprite = null;
#endif

            int index = i;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectBrush(index));
            button.name = prefab.name;
        }

        for (int i = count; i < uiButtons.buttonIcon.Length; i++)
            uiButtons.buttonIcon[i].gameObject.SetActive(false);

#if UNITY_EDITOR
        if (waitingForPreviews)
            EditorApplication.delayCall += () =>
            {
                waitingForPreviews = false;
                BuildBrushUI();
            };
#endif
    }

    void HandleCameraMovement()
    {
        if (selectedObject != null) return;

        float speed = 10f;
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) move += Vector3.back;
        if (Input.GetKey(KeyCode.A)) move += Vector3.left;
        if (Input.GetKey(KeyCode.D)) move += Vector3.right;

        mainCamera.transform.position += move * speed * Time.deltaTime;
    }

    // === Persistent object rotation ===
    void HandleRotationInput()
    {
        bool rotated = false;

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            currentRotation -= 45f;
            if (previewObject != null)
                previewObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
        }
        else if (Input.GetKeyDown(KeyCode.Period))
        {
            currentRotation += 45f;
            if (previewObject != null)
                previewObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
        }


        if (rotated && previewObject != null)
        {
            previewObject.transform.rotation = placementRotation * Quaternion.Euler(0, currentRotation, 0);
        }
    }

    // === Elevation layer switching ===
    void HandleLayerInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket)) elevation = Mathf.Max(0, elevation - 1);
        if (Input.GetKeyDown(KeyCode.RightBracket)) elevation++;
    }

    void HandlePlacement()
    {
        if (!brushActive) return;

        if (Input.GetMouseButton(0) && selectedObject == null && !editMode)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementMask))
            {
                Vector3 baseGridPos = WorldToGrid(hit.point);

                for (int x = 0; x < brushSize; x++)
                {
                    for (int z = 0; z < brushSize; z++)
                    {
                        float FloorOffset = 0;
                        //if floor move down by 1 electron foor walls
                        if (selectedBrushIndex == 0)
                        {
                            FloorOffset = 0.001f;
                        }

                        Vector3 gridPos = new Vector3(baseGridPos.x + x, elevation + FloorOffset, baseGridPos.z + z);

                        if (eraserMode)
                        {
                            if (placedObjects.ContainsKey(gridPos))
                            {
                                Destroy(placedObjects[gridPos]);
                                placedObjects.Remove(gridPos);
                                filledPositions.Remove(gridPos);
                                if (selectedGridPos == gridPos) selectedObject = null;
                            }
                        }
                        else
                        {
                            if (!filledPositions.Contains(gridPos))
                            {
                                Vector3 spawnPos = GridToWorld(gridPos);
                                GameObject obj = Instantiate(brushPrefabs[selectedBrushIndex], spawnPos, placementRotation * Quaternion.Euler(0, currentRotation, 0));
                                obj.transform.parent = this.transform;

                                filledPositions.Add(gridPos);
                                placedObjects[gridPos] = obj;
                            }
                        }
                    }
                }
            }
        }
    }

    // === Preview system ===
    void HandlePreview()
    {
        if (!brushActive || editMode) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementMask))
        {
            Vector3 baseGridPos = WorldToGrid(hit.point);

            if (baseGridPos != lastPreviewPos)
            {
                ClearPreview();

                for (int x = 0; x < brushSize; x++)
                {
                    for (int z = 0; z < brushSize; z++)
                    {
                        Vector3 gridPos = new Vector3(baseGridPos.x + x, elevation, baseGridPos.z + z);
                        Vector3 worldPos = GridToWorld(gridPos);

                        GameObject preview = Instantiate(brushPrefabs[selectedBrushIndex], worldPos, placementRotation * Quaternion.Euler(0, currentRotation, 0));
                        ApplyPreviewMaterial(preview);
                        previewObjects.Add(preview);
                    }
                }

                lastPreviewPos = baseGridPos;
            }
        }
        else
        {
            ClearPreview();
        }
    }

    void ClearPreview()
    {
        foreach (var obj in previewObjects)
            if (obj != null) Destroy(obj);

        previewObjects.Clear();
    }

    void HandleSelection()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementMask))
            {
                Vector3 gridPos = WorldToGrid(hit.point);
                if (placedObjects.ContainsKey(gridPos))
                {
                    selectedObject = placedObjects[gridPos];
                    selectedGridPos = gridPos;
                    EnterEditMode(selectedObject);
                }
            }
        }
    }

    void EnterEditMode(GameObject obj)
    {
        editMode = true;
        Vector3 pos = obj.transform.position;
        Quaternion rot = obj.transform.rotation;

        Destroy(obj);
        placedObjects.Remove(selectedGridPos);
        filledPositions.Remove(selectedGridPos);

        previewObject = Instantiate(brushPrefabs[selectedBrushIndex], pos, rot);
        ApplyPreviewMaterial(previewObject);
    }

    void HandleSelectedMovement()
    {
        if (!editMode) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementMask))
        {
            Vector3 gridPos = WorldToGrid(hit.point);

            if (gridPos != lastPreviewPos && selectedObject)
            {
                previewObject.transform.position = GridToWorld(gridPos);
                lastPreviewPos = gridPos;
            }

            if (Input.GetMouseButtonDown(0))
            {
                GameObject placed = Instantiate(brushPrefabs[selectedBrushIndex], GridToWorld(gridPos), placementRotation * Quaternion.Euler(0, currentRotation, 0));
                placedObjects[gridPos] = placed;
                filledPositions.Add(gridPos);

                Destroy(previewObject);
                previewObject = null;
                editMode = false;
                selectedObject = null;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Destroy(previewObject);
            previewObject = null;
            editMode = false;
            selectedObject = null;
        }
    }

    // === Grid placement conversions ===
    Vector3 WorldToGrid(Vector3 worldPos)
    {
        Vector3 localPos = gridOrigin.InverseTransformPoint(worldPos);
        localPos = Quaternion.Inverse(placementRotation) * localPos;

        int x = Mathf.FloorToInt(localPos.x / cellSize);
        float y = elevation;
        int z = Mathf.FloorToInt(localPos.z / cellSize);

        return new Vector3(x, y, z);
    }

    Vector3 GridToWorld(Vector3 gridPos)
    {
        Vector3 local = new Vector3(
            gridPos.x * cellSize + cellSize / 2f,
            gridPos.y * layerHeight,
            gridPos.z * cellSize + cellSize / 2f
        );

        return gridOrigin.TransformPoint(placementRotation * local);
    }

    // === Placement rotation setter ===
    public void SetPlacementAngle(float degrees)
    {
        placementAngle = degrees;
        Debug.Log($"Placement plane angle set to {degrees}°");
    }

    void ApplyPreviewMaterial(GameObject obj)
    {
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            r.material = previewMaterial;
        }
    }

    public void SelectBrush(int brushIndex)
    {
        Debug.Log("Brush selected!");
        selectedBrushIndex = Mathf.Clamp(brushIndex, 0, brushPrefabs.Length - 1);
        eraserMode = false;
        selectedObject = null;
        brushActive = true;

        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }

    public void DeselectBrush()
    {
        brushActive = false;
        selectedObject = null;
        Debug.Log("Brush deselected!");
    }

    public void ToggleEraser()
    {
        eraserMode = !eraserMode;
        selectedObject = null;
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }

    public void SetElevation(int newElevation) => elevation = newElevation;
    public void IncreaseBrushSize() => brushSize++;
    public void DecreaseBrushSize() => brushSize = Mathf.Max(1, brushSize - 1);
    public void IncreaseGridCellSize() => cellSize += 0.5f;
    public void DecreaseGridCellSize() => cellSize = Mathf.Max(0.1f, cellSize - 0.5f);

    public void ToggleCameraMode()
    {
        mainCamera.orthographic = !mainCamera.orthographic;
        if (mainCamera.orthographic)
            mainCamera.orthographicSize = 10f;
    }

    void OnDrawGizmos()
    {
        if (gridOrigin == null) return;

        Gizmos.color = eraserMode ? Color.red : Color.gray;
        float y = elevation * layerHeight;

        // Create a rotation quaternion for the placement plane
        Quaternion planeRotation = Quaternion.Euler(0, placementAngle, 0);

        // Draw X lines
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = planeRotation * new Vector3(x * cellSize, y, 0);
            Vector3 end = planeRotation * new Vector3(x * cellSize, y, gridHeight * cellSize);
            Gizmos.DrawLine(gridOrigin.TransformPoint(start), gridOrigin.TransformPoint(end));
        }

        // Draw Z lines
        for (int z = 0; z <= gridHeight; z++)
        {
            Vector3 start = planeRotation * new Vector3(0, y, z * cellSize);
            Vector3 end = planeRotation * new Vector3(gridWidth * cellSize, y, z * cellSize);
            Gizmos.DrawLine(gridOrigin.TransformPoint(start), gridOrigin.TransformPoint(end));
        }
    }

}
