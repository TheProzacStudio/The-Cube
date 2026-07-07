using UnityEngine;

public class RoomLight : MonoBehaviour
{
    private Light light;

    void Awake()
    {
        light = GetComponent<Light>();
        light.enabled = false;   // zgaszone na start
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            light.enabled = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            light.enabled = false;
    }
}