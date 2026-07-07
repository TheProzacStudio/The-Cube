using UnityEngine;

public class DungeonRenderer : MonoBehaviour
{
    [SerializeField] private int size = 4;
    [SerializeField] private float spacing = 4f;   // = szerokość pokoju

    [SerializeField] private Transform player;   // przeciągnij gracza ze sceny
    const int START = 0b00000010;

    [SerializeField] private LampManager lampManager;

    [SerializeField] private GameObject fogVolume;   // prefab mgły (Particle System) 
    [SerializeField] private float fogYOffset = 0f;

    [SerializeField] private float wallYOffset;
    [SerializeField] private float innerWallYOffset;   // dodatkowa wysokość tylko dla ścian z drzwiami
    [SerializeField] private float innerSideOffsetAlongX;   // korekta dla ścian na granicach wzdłuż X
    [SerializeField] private float innerSideOffsetAlongZ;   // korekta dla ścian na granicach wzdłuż Z

    [SerializeField] private float innerThicknessAlongX;   // korekta w poprzek dla ścian na granicach wzdłuż X
    [SerializeField] private float innerThicknessAlongZ;   // korekta w poprzek dla ścian na granicach wzdłuż Z

    [SerializeField] private GameObject centerObject;    // obiekt w środku każdego pokoju
    [SerializeField] private Vector3 centerOffset;       // korekta pozycji (np. wysokość)

    [SerializeField] private GameObject wallFull;   // ściana bez drzwi
    [SerializeField] private GameObject wallDoor;   // ściana z drzwiami
    [SerializeField] private GameObject floorFull;  // podłoga pełna
    [SerializeField] private GameObject floorHole;  // podłoga z dziurą

    [SerializeField] private float roomLightRange = 4f;
    [SerializeField] private float roomLightIntensity = 1.5f;
    [SerializeField] private float roomLightHeight = 2f;
    [SerializeField] private Color roomLightColor = Color.white;
    [SerializeField] private LightShadows roomLightShadows = LightShadows.Soft;  // cienie

    // obroty dla ścian wewnętrznych (z drzwiami)
    [SerializeField] private Vector3 wallRotationAlongX = new Vector3(90, 90, 0);
    [SerializeField] private Vector3 wallRotationAlongZ = new Vector3(90, 0, 0);

    // obroty dla ścian zewnętrznych (pełnych) — inny FBX, własna orientacja
    [SerializeField] private Vector3 outerRotationAlongX = new Vector3(0, 90, 0);
    [SerializeField] private Vector3 outerRotationAlongZ = new Vector3(0, 0, 0);

    // kolory typów
    const int THRESHOLD_DROP = 0b00001000;
    const int THRESHOLD = 0b00000100;

    void Start()
    {
        int s = Mathf.Max(4, size);   // Cube i tak wymusza min 4
        Cube cube = new Cube(s);
        cube.randomizeStart();
        cube.createThresholdPath();
        cube.calculateDifficulty();
        cube.handleTypes();

        foreach (Cube.RoomInfo r in cube.getRooms())
        {
            if (r.level != s - 1) continue;   // tylko najwyższy poziom
            bool isStart = (r.type & 0b00010) != 0;
            bool isThr = (r.type & 0b00100) != 0;
            bool isDrop = (r.type & 0b01000) != 0;
            if (isStart || isThr || isDrop)
                Debug.Log($"L{r.level} ({r.x},{r.y}) START={isStart} THRESHOLD={isThr} DROP={isDrop}");
        }

        // słownik: (level,x,y) -> RoomInfo, żeby szybko znaleźć sąsiada
        var rooms = new System.Collections.Generic.Dictionary<(int, int, int), Cube.RoomInfo>();
        foreach (Cube.RoomInfo r in cube.getRooms())
            rooms[(r.level, r.x, r.y)] = r;

        // PODŁOGI: jedna pod każdym pokojem
        foreach (Cube.RoomInfo room in cube.getRooms())
        {
            bool hole = (room.type & THRESHOLD) != 0 && room.level >= 1;
            GameObject prefab = hole ? floorHole : floorFull;

            Vector3 pos = new Vector3(
                room.x * spacing,
                room.level * spacing,
                room.y * spacing);

            Instantiate(prefab, pos, Quaternion.Euler(90, 0, 0), transform)
                .name = $"Floor L{room.level} ({room.x},{room.y}){(hole ? " HOLE" : "")}";

            // ŚWIATŁO POKOJU — zapala się gdy gracz wejdzie
            GameObject lightObj = new GameObject($"Light L{room.level} ({room.x},{room.y})");
            lightObj.transform.SetParent(transform, false);
            lightObj.transform.position = pos + new Vector3(0, roomLightHeight, 0);

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = roomLightRange;
            light.intensity = roomLightIntensity;
            light.color = roomLightColor;
            light.shadows = roomLightShadows;

            // strefa wykrywająca gracza (rozmiar pokoju)
            BoxCollider trigger = lightObj.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(spacing, spacing, spacing);
            // przesuń środek strefy do środka pokoju (światło jest podniesione)
            trigger.center = new Vector3(0, -roomLightHeight + spacing * 0.5f, 0);

            RoomZone zone = lightObj.AddComponent<RoomZone>();
            zone.level = room.level;
            zone.x = room.x;
            zone.y = room.y;

            lightObj.AddComponent<RoomLight>();

            // OBIEKT W ŚRODKU POKOJU
            if (centerObject != null)
            {
                Vector3 centerPos = pos + centerOffset;
                GameObject center = Instantiate(centerObject, centerPos, Quaternion.identity, transform);
                center.name = $"Center L{room.level} ({room.x},{room.y})";

                // DEBUG: pokoloruj wg tego samego klucza co lampy
                Color c = ColorForRoom(room);
                foreach (Renderer r in center.GetComponentsInChildren<Renderer>())
                    r.material.SetColor("_BaseColor", c);
            }

            // MGŁA POKOJU — nie dla thresholdów (są bezpieczne, przelotowe)
            bool isThreshold = (room.type & THRESHOLD) != 0 || (room.type & THRESHOLD_DROP) != 0;
            if (fogVolume != null && !isThreshold)
            {
                Vector3 fogPos = pos + new Vector3(0, spacing * 0.5f + fogYOffset, 0);
                GameObject fog = Instantiate(fogVolume, fogPos, Quaternion.identity, transform);
                fog.transform.localScale = Vector3.one * spacing * 0.95f;
                fog.name = $"Fog L{room.level} ({room.x},{room.y})";
            }
        }

        // SUFIT najwyższego poziomu — warstwa podłóg nad poziomem s-1
        for (int gx = 0; gx < s; gx++)
        {
            for (int gy = 0; gy < s; gy++)
            {
                Vector3 pos = new Vector3(
                    gx * spacing,
                    s * spacing,          // o jeden poziom wyżej niż najwyższy pokój
                    gy * spacing);

                Instantiate(floorFull, pos, Quaternion.Euler(90, 0, 0), transform)
                    .name = $"Ceiling ({gx},{gy})";
            }
        }

        // ŚCIANY: po granicach, dla każdego poziomu
        for (int level = 0; level < s; level++)
        {
            float wy = level * spacing + wallYOffset;

            // granice wzdłuż X — grubość w osi X
            for (int bx = 0; bx <= s; bx++)
            {
                bool outer = (bx == 0 || bx == s);
                GameObject prefab = outer ? wallFull : wallDoor;
                Vector3 rotEuler = outer ? outerRotationAlongX : wallRotationAlongX;
                float yy = wy + (outer ? 0f : innerWallYOffset);
                float side = outer ? 0f : innerSideOffsetAlongX;
                float thick = outer ? 0f : innerThicknessAlongX;
                for (int z = 0; z < s; z++)
                {
                    GameObject w = Instantiate(prefab,
                        new Vector3((bx - 0.5f) * spacing + thick, yy, z * spacing + side),
                        Quaternion.Euler(rotEuler), transform);

                    if (!outer)
                    {
                        SlidingDoor door = w.GetComponent<SlidingDoor>();
                        if (door != null &&
                            rooms.TryGetValue((level, bx - 1, z), out var roomA) &&
                            rooms.TryGetValue((level, bx, z), out var roomB))
                        {
                            door.SetLampColors(ColorForRoom(roomA), ColorForRoom(roomB));
                            door.SetNeighbours((level, bx - 1, z), (level, bx, z));   // <-- ta jedna linia
                            lampManager.RegisterDoor(door);
                        }
                    }
                }
            }

            // granice wzdłuż Z — grubość w osi Z
            for (int bz = 0; bz <= s; bz++)
            {
                bool outer = (bz == 0 || bz == s);
                GameObject prefab = outer ? wallFull : wallDoor;
                Vector3 rotEuler = outer ? outerRotationAlongZ : wallRotationAlongZ;
                float yy = wy + (outer ? 0f : innerWallYOffset);
                float side = outer ? 0f : innerSideOffsetAlongZ;
                float thick = outer ? 0f : innerThicknessAlongZ;
                for (int x = 0; x < s; x++)
                {
                    GameObject w = Instantiate(prefab,
                        new Vector3(x * spacing + side, yy, (bz - 0.5f) * spacing + thick),
                        Quaternion.Euler(rotEuler), transform);

                    if (!outer)
                    {
                        SlidingDoor door = w.GetComponent<SlidingDoor>();
                        if (door != null &&
                            rooms.TryGetValue((level, x, bz - 1), out var roomA) &&
                            rooms.TryGetValue((level, x, bz), out var roomB))
                        {
                            door.SetLampColors(ColorForRoom(roomA), ColorForRoom(roomB));
                            door.SetNeighbours((level, x, bz - 1), (level, x, bz));   // <-- ta jedna linia
                            lampManager.RegisterDoor(door);
                        }
                    }
                }
            }
        }

        // USTAW GRACZA w pokoju startowym
        if (player != null)
        {
            foreach (Cube.RoomInfo room in cube.getRooms())
            {
                if ((room.type & START) != 0)
                {
                    Vector3 startPos = new Vector3(
                        room.x * spacing,
                        room.level * spacing + 1f,   // +1 żeby nie utknął w podłodze
                        room.y * spacing);

                    Rigidbody rb = player.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.position = startPos;                  // przez Rigidbody, nie transform
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                    else
                    {
                        player.position = startPos;              // gdyby jednak nie miał Rigidbody
                    }

                    Debug.Log($"Gracz przeniesiony na START: {startPos}");
                    break;
                }
            }
        }

        // thresholdy i drop-y liczą się jako ukończone od początku (brak zadań)
        foreach (Cube.RoomInfo room in cube.getRooms())
        {
            if ((room.type & THRESHOLD_DROP) != 0)
                lampManager.MarkComplete((room.level, room.x, room.y));
        }

        // start liczy się jako ukończony od początku
        foreach (Cube.RoomInfo room in cube.getRooms())
        {
            if ((room.type & START) != 0)
            {
                lampManager.MarkComplete((room.level, room.x, room.y));
                break;
            }
        }
    }

    Color ColorForRoom(Cube.RoomInfo room)
    {
        // priorytet: threshold / threshold-drop = kolor specjalny
        if ((room.type & THRESHOLD) != 0 || (room.type & THRESHOLD_DROP) != 0)
            return Color.magenta;   // "specjalny" — dobierz jaki chcesz

        // inaczej: wg trudności (kolejność zgodna z enum ROOM_DIFFICULTY)
        switch (room.difficultyCategory)
        {
            case 0: return new Color(0.4f, 1f, 0.5f);    // EASY — miętowa zieleń
            case 1: return new Color(1f, 0.9f, 0.4f);    // MEDIUM — ciepły żółty
            case 2: return new Color(0.9f, 0.3f, 0.1f);    // HARD — pomarańcz
            case 3: return new Color(0.6f, 0.1f, 0.1f);    // DIABOLICAL — czerwień
            case 4: return new Color(0.8f, 0.9f, 1f);    // CHAOS — zimna biel
            default: return Color.gray;
        }
    }
}