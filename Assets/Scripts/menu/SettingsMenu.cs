using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("Panele")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject menuPanel;     // <-- główne menu (przyciski Start/Settings/Quit)

    [Header("Głośność")]
    [SerializeField] private Slider volumeSlider;

    [Header("Czułość myszy")]
    [SerializeField] private Slider sensitivitySlider;

    [Header("FPS limit")]
    [SerializeField] private TMP_Dropdown fpsDropdown;   // 30 / 60 / 120 / bez limitu
    private int[] fpsOptions = { 30, 60, 120, -1 };       // -1 = bez limitu

    [Header("Pełny ekran / VSync")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vsyncToggle;

    [Header("Rozdzielczość")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    void Start()
    {
        SetupResolutions();
        LoadSettings();
    }

    // --- otwieranie/zamykanie panelu (podepnij pod przyciski) ---
    public void OpenSettings()
    {
        if (menuPanel != null) menuPanel.SetActive(false);   // chowamy menu
        settingsPanel.SetActive(true);                        // pokazujemy ustawienia
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);                       // chowamy ustawienia
        if (menuPanel != null) menuPanel.SetActive(true);     // wracamy do menu
    }

    // --- reagowanie na zmiany (podepnij pod eventy kontrolek) ---
    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("volume", value);
    }

    public void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat("sensitivity", value);
    }

    public void OnFpsChanged(int index)
    {
        int limit = fpsOptions[index];
        Application.targetFrameRate = limit;
        PlayerPrefs.SetInt("fpsIndex", index);
    }

    public void OnFullscreenChanged(bool value)
    {
        Screen.fullScreenMode = value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        PlayerPrefs.SetInt("fullscreen", value ? 1 : 0);
    }

    public void OnVsyncChanged(bool value)
    {
        QualitySettings.vSyncCount = value ? 1 : 0;
        PlayerPrefs.SetInt("vsync", value ? 1 : 0);
    }

    public void OnResolutionChanged(int index)
    {
        Resolution r = resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        PlayerPrefs.SetInt("resIndex", index);
    }

    // --- setup listy rozdzielczości ---
    void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int current = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add($"{resolutions[i].width} x {resolutions[i].height}");
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                current = i;
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = current;
        resolutionDropdown.RefreshShownValue();
    }

    // --- wczytanie zapisanych ustawień przy starcie ---
    void LoadSettings()
    {
        // głośność
        float vol = PlayerPrefs.GetFloat("volume", 1f);
        volumeSlider.value = vol;
        AudioListener.volume = vol;

        // fps
        int fpsIdx = PlayerPrefs.GetInt("fpsIndex", 1);   // domyślnie 60
        fpsDropdown.value = fpsIdx;
        Application.targetFrameRate = fpsOptions[fpsIdx];

        bool fs = PlayerPrefs.GetInt("fullscreen", 1) == 1;
        fullscreenToggle.isOn = fs;
        Screen.fullScreenMode = fs ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        // vsync
        bool vs = PlayerPrefs.GetInt("vsync", 0) == 1;
        vsyncToggle.isOn = vs;
        QualitySettings.vSyncCount = vs ? 1 : 0;

        // czułość
        float sens = PlayerPrefs.GetFloat("sensitivity", 1f);
        sensitivitySlider.value = sens;

        // rozdzielczość
        int resIdx = PlayerPrefs.GetInt("resIndex", resolutionDropdown.value);
        if (resIdx >= 0 && resIdx < resolutions.Length)
        {
            resolutionDropdown.value = resIdx;
            Resolution r = resolutions[resIdx];
            Screen.SetResolution(r.width, r.height, Screen.fullScreen);
        }
    }
}