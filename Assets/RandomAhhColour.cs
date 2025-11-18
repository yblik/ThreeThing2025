using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAhhColour : MonoBehaviour
{
    public Material Orange;

    public MeshRenderer[] meshRenderers;

    private void Awake()
    {
        int random = Random.Range(0, 6);

        if (random == 5)
        {
            foreach (var renderer in meshRenderers)
            {
                renderer.material = Orange;
            }
        }
    }
}
