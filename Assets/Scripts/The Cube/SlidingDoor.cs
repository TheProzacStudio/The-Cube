using System.Collections;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [Header("Wings")]
    [SerializeField] private Transform leftWing;
    [SerializeField] private Transform rightWing;
    [SerializeField] private float slideDistance = 1.5f;   // how far each wing travels

    [Header("Motion timing (your jolt → pause → slow open)")]
    [SerializeField] private float joltFraction = 0.15f;   // how much of the way the quick jolt covers
    [SerializeField] private float joltTime = 0.12f;       // fast
    [SerializeField] private float pauseTime = 0.6f;       // the little stop
    [SerializeField] private float slowTime = 2.0f;        // slow full open

    [Header("Ready light")]
    [SerializeField] private float lightHeight = 2.5f;     // above the door
    [SerializeField] private Color readyColor = Color.green;

    private Vector3 leftClosed, rightClosed;
    private Light readyLight;
    private bool isReady = false;
    private bool isOpen = false;

    void Awake()
    {
        // remember closed positions (local, so it works wherever the door is placed)
        leftClosed = leftWing.localPosition;
        rightClosed = rightWing.localPosition;

        // spawn the light above the door, off for now
        GameObject lightObj = new GameObject("ReadyLight");
        lightObj.transform.SetParent(transform, false);
        lightObj.transform.localPosition = new Vector3(0, lightHeight, 0);
        readyLight = lightObj.AddComponent<Light>();
        readyLight.type = LightType.Point;
        readyLight.color = readyColor;
        readyLight.range = 6f;
        readyLight.intensity = 0f;   // off until ready
    }

    // Phase 1: task done — light on, door armed
    public void SetReady()
    {
        isReady = true;
        readyLight.intensity = 3f;
    }

    // Phase 2: player chose this door
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

        // --- quick jolt to joltFraction of the way ---
        Vector3 leftJolt = Vector3.Lerp(leftClosed, leftOpen, joltFraction);
        Vector3 rightJolt = Vector3.Lerp(rightClosed, rightOpen, joltFraction);
        yield return Slide(leftClosed, leftJolt, rightClosed, rightJolt, joltTime);

        // --- pause ---
        yield return new WaitForSeconds(pauseTime);

        // --- slow open the rest of the way ---
        yield return Slide(leftJolt, leftOpen, rightJolt, rightOpen, slowTime);
    }

    // slides both wings from their start to end over 'duration'
    private IEnumerator Slide(Vector3 lFrom, Vector3 lTo, Vector3 rFrom, Vector3 rTo, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);   // eases in/out
            leftWing.localPosition = Vector3.Lerp(lFrom, lTo, k);
            rightWing.localPosition = Vector3.Lerp(rFrom, rTo, k);
            yield return null;
        }
        leftWing.localPosition = lTo;
        rightWing.localPosition = rTo;
    }
}