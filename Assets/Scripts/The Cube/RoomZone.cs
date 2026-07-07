using UnityEngine;

public class RoomZone : MonoBehaviour
{
    [HideInInspector] public int level, x, y;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            PlayerRoomTracker.currentRoom = (level, x, y);
    }
}