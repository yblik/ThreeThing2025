using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShipBuoyancy : MonoBehaviour
{
    public Transform[] floatPoints;
    public float buoyancyForce = 3f;          // Scales with mass
    public float baseWaterHeight = 2.680548f;

    public float waveStrength = 0.5f;
    public float waveSpeed = 1f;
    public float waveFrequency = 2f;

    public float waterDrag = 0.5f;
    public float waterAngularDrag = 0.5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        foreach (var fp in floatPoints)
        {
            float waterHeight = GetWaveHeight(fp.position);

            if (fp.position.y < waterHeight)
            {
                float depth = waterHeight - fp.position.y;

                // More realistic buoyancy – scales by object's mass
                float force = buoyancyForce * depth * rb.mass;

                rb.AddForceAtPosition(Vector3.up * force, fp.position);

                rb.AddForce(-rb.velocity * waterDrag);
                rb.AddTorque(-rb.angularVelocity * waterAngularDrag);
            }
        }
    }

    float GetWaveHeight(Vector3 pos)
    {
        float wave =
            Mathf.Sin(Time.time * waveSpeed + pos.x * waveFrequency) * waveStrength +
            Mathf.Cos(Time.time * waveSpeed + pos.z * waveFrequency) * waveStrength;

        return baseWaterHeight + wave;
    }
}
