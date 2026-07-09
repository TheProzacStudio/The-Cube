using System.Collections;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [Header("Wings")]
    [SerializeField] private Transform leftWing;
    [SerializeField] private Transform rightWing;
    [SerializeField] private float slideDistance = 1.5f;

    [Header("Motion timing (jolt → pause → slow open)")]
    [SerializeField] private float joltFraction = 0.15f;
    [SerializeField] private float joltTime = 0.12f;
    [SerializeField] private float pauseTime = 0.6f;
    [SerializeField] private float slowTime = 2.0f;

    [Header("Dźwięki drzwi")]
    [SerializeField] private AudioSource audioSource;   // jolt + łup
    [SerializeField] private AudioSource slideSource;   // osobny, dla suwu (fade out)
    [SerializeField] private AudioClip joltClip;
    [SerializeField] private AudioClip slideClip;
    [SerializeField] private AudioClip endClip;
    [SerializeField] private float slideFadeTime = 0.4f;

    [Header("Lamps (po jednej z każdej strony)")]
    [SerializeField] private DoorLamp lampSideA;
    [SerializeField] private DoorLamp lampSideB;

    private Vector3 leftClosed, rightClosed;
    private bool isReady = false;
    private bool isOpen = false;

    private (int, int, int) roomA;
    private (int, int, int) roomB;

    public void SetNeighbours((int, int, int) a, (int, int, int) b)
    {
        roomA = a;
        roomB = b;
    }

    public void RefreshLamps(LampManager manager)
    {
        bool aDone = manager.IsComplete(roomA);
        bool bDone = manager.IsComplete(roomB);

        // lampSideA fizycznie po stronie B → reaguje na bDone
        if (lampSideA != null)
        {
            if (bDone && !aDone) lampSideA.TurnOn();
            else lampSideA.TurnOff();
        }
        if (lampSideB != null)
        {
            if (aDone && !bDone) lampSideB.TurnOn();
            else lampSideB.TurnOff();
        }
    }

    void Awake()
    {
        leftClosed = leftWing.localPosition;
        rightClosed = rightWing.localPosition;

        if (lampSideA != null) lampSideA.TurnOff();
        if (lampSideB != null) lampSideB.TurnOff();
    }

    public void SetReady()
    {
        isReady = true;
    }

    public void Open()
    {
        if (!isReady || isOpen) return;
        isOpen = true;
        StartCoroutine(OpenSequence());
    }

    private IEnumerator OpenSequence()
    {
        Vector3 leftOpen = leftClosed + Vector3.left * slideDistance;
        Vector3 rightOpen = rightClosed + Vector3.right * slideDistance;

        Vector3 leftJolt = Vector3.Lerp(leftClosed, leftOpen, joltFraction);
        Vector3 rightJolt = Vector3.Lerp(rightClosed, rightOpen, joltFraction);

        // klęk przy jolcie
        if (audioSource != null && joltClip != null)
            audioSource.PlayOneShot(joltClip);
        yield return Slide(leftClosed, leftJolt, rightClosed, rightJolt, joltTime);

        // pauza
        yield return new WaitForSeconds(pauseTime);

        // suw — osobne źródło, gra przez całą fazę otwierania
        if (slideSource != null && slideClip != null)
        {
            slideSource.clip = slideClip;
            slideSource.loop = true;
            slideSource.Play();
        }
        yield return Slide(leftJolt, leftOpen, rightJolt, rightOpen, slowTime);

        // suw wycisza się (fade out), równolegle łup
        if (slideSource != null && slideSource.isPlaying)
            StartCoroutine(FadeOutSlide());
        if (audioSource != null && endClip != null)
            audioSource.PlayOneShot(endClip);
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

    private IEnumerator Slide(Vector3 lFrom, Vector3 lTo, Vector3 rFrom, Vector3 rTo, float duration)
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

    public void SetLampColors(Color colorA, Color colorB)
    {
        // lampSideA świeci gdy B ukończony → ma pokazywać kolor A (nieukończonego sąsiada)
        if (lampSideA != null) lampSideA.SetColor(colorA);
        if (lampSideB != null) lampSideB.SetColor(colorB);
    }
}