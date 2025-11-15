using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ChoppyWater : MonoBehaviour
{
    public float waveStrength = 0.5f;
    public float waveSpeed = 1f;
    public float waveFrequency = 1.5f;
    public float randomFactor = 0.5f;

    private Mesh mesh;
    private Vector3[] baseVertices;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        baseVertices = mesh.vertices;
    }

    void Update()
    {
        Vector3[] vertices = new Vector3[baseVertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = baseVertices[i];

            float wave =
                Mathf.Sin((Time.time * waveSpeed) + (v.x + v.z) * waveFrequency) +
                Mathf.Cos((Time.time * (waveSpeed * 1.4f)) + v.z * waveFrequency * 0.6f);

            // Random offset to reduce uniformity
            wave += Mathf.Sin(v.x * 2 + v.z * 3 + Time.time * waveSpeed) * randomFactor;

            v.y += wave * waveStrength;

            vertices[i] = v;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
