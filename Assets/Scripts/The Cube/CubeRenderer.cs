using UnityEngine;

public class DungeonRenderer : MonoBehaviour
{
    [SerializeField] private int size = 4;
    [SerializeField] private float spacing = 4f;   // = szerokość pokoju

    [SerializeField] private float wallYOffset;
    [SerializeField] private float innerWallYOffset;   // dodatkowa wysokość tylko dla ścian z drzwiami
    [SerializeField] private float innerSideOffsetAlongX;   // korekta dla ścian na granicach wzdłuż X
    [SerializeField] private float innerSideOffsetAlongZ;   // korekta dla ścian na granicach wzdłuż Z

    [SerializeField] private float innerThicknessAlongX;   // korekta w poprzek dla ścian na granicach wzdłuż X
    [SerializeField] private float innerThicknessAlongZ;   // korekta w poprzek dla ścian na granicach wzdłuż Z

    [SerializeField] private GameObject wallFull;   // ściana bez drzwi
    [SerializeField] private GameObject wallDoor;   // ściana z drzwiami
    [SerializeField] private GameObject floorFull;  // podłoga pełna
    [SerializeField] private GameObject floorHole;  // podłoga z dziurą

    // obroty dla ścian wewnętrznych (z drzwiami)
    [SerializeField] private Vector3 wallRotationAlongX = new Vector3(90, 90, 0);
    [SerializeField] private Vector3 wallRotationAlongZ = new Vector3(90, 0, 0);

    // obroty dla ścian zewnętrznych (pełnych) — inny FBX, własna orientacja
    [SerializeField] private Vector3 outerRotationAlongX = new Vector3(0, 90, 0);
    [SerializeField] private Vector3 outerRotationAlongZ = new Vector3(0, 0, 0);

    const int THRESHOLD_DROP = 0b00001000;

    void Start()
    {
        int s = Mathf.Max(4, size);   // Cube i tak wymusza min 4
        Cube cube = new Cube(s);
        cube.randomizeStart();
        cube.createThresholdPath();
        cube.calculateDifficulty();
        cube.handleTypes();

        // PODŁOGI: jedna pod każdym pokojem
        foreach (Cube.RoomInfo room in cube.getRooms())
        {
            bool hole = (room.type & THRESHOLD_DROP) != 0 && room.level >= 1;
            GameObject prefab = hole ? floorHole : floorFull;

            Vector3 pos = new Vector3(
                room.x * spacing,
                room.level * spacing,
                room.y * spacing);

            Instantiate(prefab, pos, Quaternion.Euler(90, 0, 0), transform)
                .name = $"Floor L{room.level} ({room.x},{room.y}){(hole ? " HOLE" : "")}";
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
                    Instantiate(prefab,
                        new Vector3((bx - 0.5f) * spacing + thick, yy, z * spacing + side),
                        Quaternion.Euler(rotEuler), transform);
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
                    Instantiate(prefab,
                        new Vector3(x * spacing + side, yy, (bz - 0.5f) * spacing + thick),
                        Quaternion.Euler(rotEuler), transform);
            }
        }
    }
}