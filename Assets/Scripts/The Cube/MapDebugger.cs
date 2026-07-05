using System.IO;
using UnityEngine;

public class MapDebugger : MonoBehaviour
{
    [SerializeField] private int size = 5;
    private string path;

    void Start()
    {
        path = Path.Combine(Application.persistentDataPath, "mapdebug.txt");
        File.WriteAllText(path, "");                 // czyści plik na start
        Application.logMessageReceived += ToFile;    // łapie każdy Debug.Log
        Debug.Log($"Plik z logami: {path}");

        Debug.Log($"=== Generowanie mapy, size={size} ===");

        try
        {
            Cube cube = new Cube(size);
            Debug.Log("Cube utworzony");

            cube.randomizeStart();
            Debug.Log("randomizeStart OK");

            cube.createThresholdPath();
            Debug.Log("createThresholdPath OK");

            cube.calculateDifficulty();
            Debug.Log("calculateDifficulty OK");

            cube.handleTypes();
            Debug.Log("handleTypes OK");

            cube.debugPrint();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Wywaliło się na: {e.Message}\n{e.StackTrace}");
        }
    }

    void ToFile(string message, string stackTrace, LogType type)
    {
        File.AppendAllText(path, message + "\n");
    }

    void OnDisable()
    {
        Application.logMessageReceived -= ToFile;    // sprzątanie
    }
}