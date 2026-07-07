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

    [Header("Lamps (po jednej z każdej strony)")]
    [SerializeField] private DoorLamp lampSideA;   // patrzy w stronę pokoju A
    [SerializeField] private DoorLamp lampSideB;   // patrzy w stronę pokoju B

    private Vector3 leftClosed, rightClosed;
    private Light readyLight;
    private bool isReady = false;
    private bool isOpen = false;

    // które dwa pokoje rozdzielają te drzwi (zapisywane przy spawnie)
    private (int, int, int) roomA;   // pokój po jednej stronie
    private (int, int, int) roomB;   // pokój po drugiej stronie

    public void SetNeighbours((int, int, int) a, (int, int, int) b)
    {
        roomA = a;
        roomB = b;
    }

    public void RefreshLamps(LampManager manager)
    {
        bool aDone = manager.IsComplete(roomA);
        bool bDone = manager.IsComplete(roomB);

        // ZAMIENIONE: lampSideA świeci gdy roomB ukończony (bo fizycznie wisi po stronie B)
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
        // remember closed positions (local, so it works wherever the door is placed)
        leftClosed = leftWing.localPosition;
        rightClosed = rightWing.localPosition;

        if (lampSideA != null) lampSideA.TurnOff();
        if (lampSideB != null) lampSideB.TurnOff();
    }

    // Phase 1: task done — light on, door armed
    public void SetReady()
    {
        isReady = true;
        //if (lampSideA != null) lampSideA.TurnOn();
        //if (lampSideB != null) lampSideB.TurnOn();
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

    // wołane przez renderer: colorA = kolor pokoju po stronie A, colorB = po stronie B
    public void SetLampColors(Color colorA, Color colorB)
    {
        if (lampSideA != null) lampSideA.SetColor(colorA);
        if (lampSideB != null) lampSideB.SetColor(colorB);
    }
}