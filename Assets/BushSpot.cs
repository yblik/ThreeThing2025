using UnityEngine;

public class BushSpot : MonoBehaviour
{
    [HideInInspector] public bool isOccupied = false;
    public float hideOffsetY = 0.1f;

    private BushGroupManager parentGroup;

    void Awake()
    {
        parentGroup = GetComponentInParent<BushGroupManager>();
        if (parentGroup)
            parentGroup.RegisterBush(this);
    }

    public Vector3 GetHidePosition()
    {
        Vector3 pos = transform.position;
        pos.y += hideOffsetY;
        return pos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isOccupied ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
