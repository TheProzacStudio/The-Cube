using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuIntro : MonoBehaviour
{
    [Header("Drzwi — ruch jolt → pauza → wolno")]
    [SerializeField] private Transform leftWing;
    [SerializeField] private Transform rightWing;
    [SerializeField] private float slideDistance = 1.5f;
    [SerializeField] private float joltFraction = 0.15f;   // jak daleko idzie szybki jolt
    [SerializeField] private float joltTime = 0.12f;       // szybko
    [SerializeField] private float doorPauseTime = 0.6f;   // stop między joltem a resztą
    [SerializeField] private float slowTime = 2.0f;        // wolne dokończenie

    [SerializeField] private DoorLamp doorLamp;

    [Header("Kamera — wjazd w drzwi")]
    [SerializeField] private Transform cameraToMove;
    [SerializeField] private Vector3 cameraMoveBy = new Vector3(0, 0, 10); // dokąd przejechać (względnie)
    [SerializeField] private float cameraMoveTime = 3f;

    [Header("Wyciemnianie")]
    [SerializeField] private UnityEngine.UI.Image fadeOverlay;   // czarny obraz na Canvasie

    [Header("Czasy")]
    [SerializeField] private float pauseBeforeCamera = 0.5f;  // pauza między drzwiami a kamerą

    [Header("Zmiana sceny po intro (opcjonalne)")]
    [SerializeField] private bool loadSceneAfter = false;
    [SerializeField] private string sceneToLoad = "";

    private bool started = false;

    public void Start()
    {
        doorLamp.SetColor(new Color(0.6f, 0.1f, 0.1f));
        if (doorLamp != null) doorLamp.TurnOn();
    }

    // podepnij to pod przycisk Start (OnClick)
    public void PlayIntro()
    {
        if (started) return;
        started = true;
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        // 1. drzwi — jolt, pauza, wolne otwarcie (jak w grze)
        Vector3 lClosed = leftWing.localPosition;
        Vector3 rClosed = rightWing.localPosition;
        Vector3 lOpen = lClosed + Vector3.left * slideDistance;
        Vector3 rOpen = rClosed + Vector3.right * slideDistance;

        // jolt do joltFraction drogi
        Vector3 lJolt = Vector3.Lerp(lClosed, lOpen, joltFraction);
        Vector3 rJolt = Vector3.Lerp(rClosed, rOpen, joltFraction);
        yield return SlideWings(lClosed, lJolt, rClosed, rJolt, joltTime);

        // pauza
        yield return new WaitForSeconds(doorPauseTime);

        // wolne dokończenie
        yield return SlideWings(lJolt, lOpen, rJolt, rOpen, slowTime);

        // 2. pauza przed kamerą
        yield return new WaitForSeconds(pauseBeforeCamera);

        // 3. kamera wjeżdża + jednoczesne wyciemnianie
        if (cameraToMove != null)
        {
            Vector3 camStart = cameraToMove.position;
            Vector3 camEnd = camStart + cameraToMove.TransformDirection(cameraMoveBy);
            float t = 0f;
            while (t < cameraMoveTime)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / cameraMoveTime);
                cameraToMove.position = Vector3.Lerp(camStart, camEnd, k);

                // fade: alfa rośnie od 0 do 1 razem z ruchem
                if (fadeOverlay != null)
                {
                    Color c = fadeOverlay.color;
                    c.a = k;
                    fadeOverlay.color = c;
                }
                yield return null;
            }

            // upewnij się, że na końcu pełna czerń
            if (fadeOverlay != null)
            {
                Color c = fadeOverlay.color;
                c.a = 1f;
                fadeOverlay.color = c;
            }
        }

        // 4. opcjonalnie załaduj scenę gry
        if (loadSceneAfter && !string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }

    private IEnumerator SlideWings(Vector3 lFrom, Vector3 lTo, Vector3 rFrom, Vector3 rTo, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            leftWing.localPosition = Vector3.Lerp(lFrom, lTo, k);
            rightWing.localPosition = Vector3.Lerp(rFrom, rTo, k);
            yield return null;
        }
        leftWing.localPosition = lTo;
        rightWing.localPosition = rTo;
    }
}