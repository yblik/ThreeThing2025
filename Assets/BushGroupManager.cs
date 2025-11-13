using System.Collections.Generic;
using UnityEngine;

public class BushGroupManager : MonoBehaviour
{
    private List<BushSpot> bushes = new();

    public void RegisterBush(BushSpot bush)
    {
        if (!bushes.Contains(bush))
            bushes.Add(bush);
    }

    public BushSpot GetAvailableBush(Vector3 fromPosition, float maxDistance)
    {
        BushSpot nearest = null;
        float minDist = Mathf.Infinity;

        foreach (BushSpot bush in bushes)
        {
            if (bush.isOccupied) continue;

            float dist = Vector3.Distance(fromPosition, bush.transform.position);
            if (dist < minDist && dist <= maxDistance)
            {
                minDist = dist;
                nearest = bush;
            }
        }

        return nearest;
    }

    public void ReleaseBush(BushSpot bush)
    {
        if (bush)
            bush.isOccupied = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 0.2f, 1));
    }
}

