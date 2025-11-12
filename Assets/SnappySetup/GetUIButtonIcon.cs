using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class GetUIButtonIcon : MonoBehaviour
{
    [Header("All buttons found in children")]
    public Button[] buttonIcon;

    // Automatically populate list when script is loaded or changed
    void OnValidate()
    {
        RefreshButtons();
    }

    [ContextMenu("Refresh Button List")]
    public void RefreshButtons()
    {
        buttonIcon = GetComponentsInChildren<Button>(true);
    }
}
