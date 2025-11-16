using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SnakeSlither : MonoBehaviour
{
    [Header("Segment Setup")]
    public Transform[] segments;         // Ordered from head to tail
    public float segmentSpacing = 0.45f; // Perfect spacing for short segments

    [Header("Wave Settings")]
    public float amplitude = 0.06f;      // Side-to-side wiggle
    public float wavelength = 0.45f;     // How fast the wave moves down body
    public float waveSpeed = 1.2f;       // Speed of wave animation

    void LateUpdate()
    {
        if (segments.Length < 2)
            return;

        float time = Time.time * waveSpeed;

        // HEAD segment is moved externally (AI / NavMesh / player)
        Transform head = segments[0];

        // FOLLOWERS
        for (int i = 1; i < segments.Length; i++)
        {
            Transform curr = segments[i];
            Transform prev = segments[i - 1];

            // --------------------------------------------------------
            // 1. Calculate direction from current segment to previous
            // --------------------------------------------------------
            Vector3 toPrev = prev.position - curr.position;
            Vector3 dir = toPrev.normalized;

            // --------------------------------------------------------
            // 2. Compute the correct follow position at exact spacing
            // --------------------------------------------------------
            Vector3 targetPos = prev.position - dir * segmentSpacing;

            // --------------------------------------------------------
            // 3. Apply sine-wave SLITHER in *local perpendicular* dir
            //    (this prevents spiraling and spinning circles)
            // --------------------------------------------------------
            Vector3 right = Vector3.Cross(Vector3.up, dir);
            float wavePhase = time + (i * wavelength);
            float waveOffset = Mathf.Sin(wavePhase) * amplitude;

            targetPos += right * waveOffset;

            // --------------------------------------------------------
            // 4. Smooth snapping (keeps spacing, no overlap, no jitter)
            // --------------------------------------------------------
            curr.position = Vector3.Lerp(curr.position, targetPos, Time.deltaTime * 20f);

            // --------------------------------------------------------
            // 5. Rotate segment to face the previous one
            // --------------------------------------------------------
            Vector3 lookDir = prev.position - curr.position;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion desiredRot = Quaternion.LookRotation(lookDir, Vector3.up);
                curr.rotation = Quaternion.Slerp(curr.rotation, desiredRot, Time.deltaTime * 12f);
            }
        }
    }
}