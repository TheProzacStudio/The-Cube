using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuIntro : MonoBehaviour
{
    [Header("Drzwi — ruch jolt → pauza → wolno")]
    [SerializeField] private Transform leftWing;
    [SerializeField] private Transform rightWing;
    [SerializeField] private float slideDistance = 1.5f;
    [SerializeField] private float joltFraction = 0.15f;
    [SerializeField] private float joltTime = 0.12f;
    [SerializeField] private float doorPauseTime = 0.6f;
    [SerializeField] private float slowTime = 2.0f;

    [Header("Dźwięki drzwi")]
    [SerializeField] private AudioSource audioSource;   // jolt + łup
    [SerializeField] private AudioSource slideSource;   // osobny, dla suwu (fade out)
    [SerializeField] private AudioClip joltClip;
    [SerializeField] private AudioClip slideClip;
    [SerializeField] private AudioClip endClip;
    [SerializeField] private float slideFadeTime = 0.4f;

    [Header("Lampa nad drzwiami")]
    [SerializeField] private DoorLamp doorLamp;
    [SerializeField] private Color lampColor = new Color(0.6f, 0.1f, 0.1f);

    [Header("Kamera — wjazd w drzwi")]
    [SerializeField] private Transform cameraToMove;
    [SerializeField] private Vector3 cameraMoveBy = new Vector3(0, 0, 10);
    [SerializeField] private float cameraMoveTime = 3f;

    [Header("Czasy")]
    [SerializeField] private float pauseBeforeCamera = 0.5f;

    [Header("Wyciemnianie")]
    [SerializeField] private UnityEngine.UI.Image fadeOverlay;

    [Header("Zmiana sceny po intro (opcjonalne)")]
    [SerializeField] private bool loadSceneAfter = false;
    [SerializeField] private string sceneToLoad = "";

    private bool started = false;

    void Start()
    {
        if (doorLamp != null)
        {
            doorLamp.SetColor(lampColor);
            doorLamp.TurnOn();
        }
    }

    public void PlayIntro()
    {
        if (started) return;
        started = true;
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        // 1. drzwi — jolt, pauza, wolne otwarcie + dźwięki
        Vector3 lClosed = leftWing.localPosition;
        Vector3 rClosed = rightWing.localPosition;
        Vector3 lOpen = lClosed + Vector3.left * slideDistance;
        Vector3 rOpen = rClosed + Vector3.right * slideDistance;

        Vector3 lJolt = Vector3.Lerp(lClosed, lOpen, joltFraction);
        Vector3 rJolt = Vector3.Lerp(rClosed, rOpen, joltFraction);

        // klęk przy jolcie
        if (audioSource != null && joltClip != null)
            audioSource.PlayOneShot(joltClip);
        yield return SlideWings(lClosed, lJolt, rClosed, rJolt, joltTime);

        // pauza
        yield return new WaitForSeconds(doorPauseTime);

        // suw — osobne źródło, gra przez całą fazę otwierania
        if (slideSource != null && slideClip != null)
        {
            slideSource.clip = slideClip;
            slideSource.loop = true;
            slideSource.volume = 1f;
            slideSource.Play();
        }

        // wolne dokończenie otwierania (suw gra w tle)
        yield return SlideWings(lJolt, lOpen, rJolt, rOpen, slowTime);

        // suw wycisza się (fade out), równolegle łup
        if (slideSource != null && slideSource.isPlaying)
            StartCoroutine(FadeOutSlide());
        if (audioSource != null && endClip != null)
            audioSource.PlayOneShot(endClip);

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

                if (fadeOverlay != null)
                {
                    Color c = fadeOverlay.color;
                    c.a = k;
                    fadeOverlay.color = c;
                }
                yield return null;
            }

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

    private IEnumerator FadeOutSlide()
    {
        float startVol = slideSource.volume;
        float t = 0f;
        while (t < slideFadeTime)
        {
            t += Time.deltaTime;
            slideSource.volume = Mathf.Lerp(startVol, 0f, t / slideFadeTime);
            yield return null;
        }
        slideSource.volume = 0f;
        slideSource.Stop();
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