using System.Collections;
using UnityEngine;

public class ThresholdHatch : MonoBehaviour
{
    [Header("Wings")]
    [SerializeField] private Transform leftWing;
    [SerializeField] private Transform rightWing;
    [SerializeField] private float slideDistance = 1.5f;

    [Header("Motion timing (jolt → pauza → wolno)")]
    [SerializeField] private float joltFraction = 0.15f;
    [SerializeField] private float joltTime = 0.12f;
    [SerializeField] private float pauseTime = 0.6f;
    [SerializeField] private float slowTime = 2.0f;

    private Vector3 leftClosed, rightClosed;
    private bool isOpen = false;

    void Awake()
    {
        leftClosed = leftWing.localPosition;
        rightClosed = rightWing.localPosition;
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        StartCoroutine(OpenSequence());
    }

    private IEnumerator OpenSequence()
    {
        // skrzydła rozsuwają się wzdłuż lokalnej osi X (klapa leży poziomo)
        Vector3 leftOpen = leftClosed + Vector3.left * slideDistance;
        Vector3 rightOpen = rightClosed + Vector3.right * slideDistance;

        Vector3 leftJolt = Vector3.Lerp(leftClosed, leftOpen, joltFraction);
        Vector3 rightJolt = Vector3.Lerp(rightClosed, rightOpen, joltFraction);
        yield return Slide(leftClosed, leftJolt, rightClosed, rightJolt, joltTime);

        yield return new WaitForSeconds(pauseTime);

        yield return Slide(leftJolt, leftOpen, rightJolt, rightOpen, slowTime);
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
}