using UnityEngine;

public class DoorLamp : MonoBehaviour
{
    [SerializeField] private Renderer lampVisual;
    [SerializeField] private int glowMaterialIndex = 0;
    [SerializeField] private float litIntensity = 2f;
    [SerializeField] private Light lampLight;
    [SerializeField] private float lightIntensity = 3f;

    [Header("Migotanie (opcjonalne)")]
    [SerializeField] private bool flicker = false;         // czy ta lampa miga
    [SerializeField] private float flickerAmount = 1.5f;   // jak mocno
    [SerializeField] private float flickerSpeed = 12f;     // jak szybko

    private Color color = Color.white;
    private bool isOn = false;
    private Material Mat => lampVisual.materials[glowMaterialIndex];

    void Awake()
    {
        if (lampLight != null) lampLight.enabled = false;
    }

    void Update()
    {
        // migotanie tylko gdy lampa świeci i ma włączony flicker
        if (!flicker || !isOn || lampLight == null) return;

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        float intensity = lightIntensity + (noise - 0.5f) * 2f * flickerAmount;
        lampLight.intensity = intensity;

        // emisja też pulsuje razem ze światłem
        if (lampVisual != null)
        {
            float emFactor = litIntensity * (intensity / lightIntensity);
            Mat.SetColor("_EmissionColor", color * emFactor);
        }
    }

    public void SetColor(Color c)
    {
        color = c;
        if (lampVisual != null) Mat.SetColor("_BaseColor", c);
        if (lampLight != null) lampLight.color = c;
    }

    public void TurnOff()
    {
        isOn = false;
        if (lampVisual != null) Mat.SetColor("_EmissionColor", Color.black);
        if (lampLight != null) lampLight.enabled = false;
    }

    public void TurnOn()
    {
        isOn = true;
        if (lampVisual != null)
        {
            Mat.EnableKeyword("_EMISSION");
            Mat.SetColor("_EmissionColor", color * litIntensity);
        }
        if (lampLight != null)
        {
            lampLight.enabled = true;
            lampLight.intensity = lightIntensity;
        }
    }
}