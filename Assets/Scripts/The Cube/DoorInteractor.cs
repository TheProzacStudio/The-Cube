using UnityEngine;
using UnityEngine.InputSystem;   // nowy Input System

public class DoorInteractor : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float reach = 4f;

    void Start()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        // klawiatura z nowego Input System; sprawdzamy klawisz E
        if (Keyboard.current == null) return;
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;

        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, reach))
        {
            SlidingDoor door = hit.collider.GetComponentInParent<SlidingDoor>();
            if (door != null)
            {
                door.SetReady();
                door.Open();
                return;
            }

            ThresholdHatch hatch = hit.collider.GetComponentInParent<ThresholdHatch>();
            if (hatch != null)
            {
                hatch.Open();
            }
        }
    }
}