using UnityEngine;

public class CeilingLightController : MonoBehaviour
{
    private Light pointLight; // Reference to the Point Light component
    private Renderer lightRenderer; // Reference to the Renderer of the fluorescent light
    public Color lightOnEmission = Color.white; // Emission color when the light is on
    public Color lightOffEmission = Color.black; // Emission color when the light is off
    public bool isLightOn = true;

    void Start()
    {
        // Dynamically find the components within the prefab
        pointLight = GetComponentInChildren<Light>();
        lightRenderer = GetComponentInChildren<Renderer>();

        if (pointLight == null || lightRenderer == null)
        {
            Debug.LogError("CeilingLightController: Point Light or Renderer not found in prefab.");
            enabled = false; // Disable script if dependencies are missing
        }
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleLight();
        }*/
    }

    public void ToggleLight()
    {
        isLightOn = !isLightOn;

        // Toggle the Point Light
        pointLight.enabled = isLightOn;

        // Adjust the emission color
        Material material = lightRenderer.material;

        // Enable or disable the emission keyword
        if (isLightOn)
        {
            material.EnableKeyword("_EMISSION");
        }
        else
        {
            material.DisableKeyword("_EMISSION");
        }
    }

    public void ToggleLightOn()
    {
        // Toggle the Point Light
        pointLight.enabled = true;

        // Adjust the emission color
        Material material = lightRenderer.material;

        material.EnableKeyword("_EMISSION");
    }

    public void ToggleLightOff()
    {
        // Toggle the Point Light
        pointLight.enabled = false;

        // Adjust the emission color
        Material material = lightRenderer.material;

        material.DisableKeyword("_EMISSION");
    }
}
