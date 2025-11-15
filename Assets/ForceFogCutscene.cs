using UnityEngine;

public class ForceFog : MonoBehaviour
{
    void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.6f, 0.7f, 0.8f);
        RenderSettings.fogDensity = 0.02f;
    }
}

