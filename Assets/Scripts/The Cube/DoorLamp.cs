using UnityEngine;

public class DoorLamp : MonoBehaviour
{
    [SerializeField] private Renderer lampVisual;
    [SerializeField] private int glowMaterialIndex = 0;
    [SerializeField] private float litIntensity = 2f;
    [SerializeField] private Light lampLight;        // <-- Point Light lampy
    [SerializeField] private float lightIntensity = 3f;

    private Color color = Color.white;
    private Material Mat => lampVisual.materials[glowMaterialIndex];

    void Awake()
    {
        if (lampLight != null) lampLight.enabled = false;   // zgaszone na start
    }

    public void SetColor(Color c)
    {
        color = c;
        if (lampVisual != null) Mat.SetColor("_BaseColor", c);
        if (lampLight != null) lampLight.color = c;
    }

    public void TurnOff()
    {
        if (lampVisual != null) Mat.SetColor("_EmissionColor", Color.black);
        if (lampLight != null) lampLight.enabled = false;
    }

    public void TurnOn()
    {

        if (lampVisual != null)
        {
            Mat.EnableKeyword("_EMISSION");
            Mat.SetColor("_EmissionColor", color * litIntensity);
        }
        if (lampLight != null)
        {
            lampLight.enabled = true;
            lampLight.intensity = lightIntensity;
            Debug.Log($"{name} światło ON, intensity={lampLight.intensity}, range={lampLight.range}");
        }
        else Debug.LogWarning($"{name} lampLight NIE podpięty!");
    }
}