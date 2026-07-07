using System.Collections.Generic;
using UnityEngine;

public class LampManager : MonoBehaviour
{
    // lista wszystkich drzwi w mapie
    private List<SlidingDoor> doors = new List<SlidingDoor>();

    // lista ukończonych pokoi (te trzy liczby = jeden pokój)
    private HashSet<(int, int, int)> completed = new HashSet<(int, int, int)>();

    // renderer rejestruje tu każde drzwi przy spawnie
    public void RegisterDoor(SlidingDoor door)
    {
        doors.Add(door);
    }

    // twój system zadań woła to, gdy pokój zostaje ukończony
    public void MarkComplete((int, int, int) room)
    {
        completed.Add(room);
        RefreshAllLamps();
    }

    // sprawdzenie czy pokój jest ukończony
    public bool IsComplete((int, int, int) room)
    {
        return completed.Contains(room);
    }

    // każe wszystkim drzwiom przeliczyć swoje lampy
    private void RefreshAllLamps()
    {
        foreach (SlidingDoor door in doors)
            door.RefreshLamps(this);
    }
}