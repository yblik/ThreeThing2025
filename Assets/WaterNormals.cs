using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterNormals : MonoBehaviour
{
    public Material mat;
    public float scrollSpeedU = 0.1f;
    public float scrollSpeedV = 0.05f;

    private Vector2 offset;

    void Update()
    {
        offset.x += scrollSpeedU * Time.deltaTime;
        offset.y += scrollSpeedV * Time.deltaTime;

        mat.SetTextureOffset("_BumpMap", offset); // For Standard Shader
    }
}
