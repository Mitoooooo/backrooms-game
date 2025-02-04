using UnityEngine;

public class LightCulling : MonoBehaviour
{
    public float cullingDistance = 20f; // Distance within which lights remain active
    private GameObject[] lights;

    void Start()
    {
        // Find all lights with the "LightSource" tag
        lights = GameObject.FindGameObjectsWithTag("LightSource");
    }

    void Update()
    {
        foreach (GameObject lightObject in lights)
        {
            if (lightObject.TryGetComponent(out Light light))
            {
                float distance = Vector3.Distance(transform.position, lightObject.transform.position);

                // Enable or disable light based on distance
                light.enabled = distance <= cullingDistance;
            }
        }
    }
}