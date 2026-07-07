using UnityEngine;
using UnityEngine.InputSystem;

public class TaskCompleter : MonoBehaviour
{
    [SerializeField] private LampManager lampManager;

    void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.qKey.wasPressedThisFrame) return;

        var room = PlayerRoomTracker.currentRoom;
        if (room.Item1 >= 0)
        {
            lampManager.MarkComplete(room);
            Debug.Log($"Ukończono pokój {room}");
        }
        else Debug.Log("Nie jesteś w żadnym pokoju");
    }
}