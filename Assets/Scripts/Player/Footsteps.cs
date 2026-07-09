using UnityEngine;

public class Footsteps : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] stepClips;   // kilka wariantów do losowania

    [Header("Tempo")]
    [SerializeField] private float minSpeed = 0.5f;       // poniżej tej prędkości = brak kroków
    [SerializeField] private float stepDistance = 2.5f;   // co ile "przebytej drogi" krok
    [SerializeField] private float pitchVariation = 0.1f; // lekka losowa zmiana wysokości

    private float accumulated = 0f;

    void Update()
    {
        if (movement == null || source == null || stepClips.Length == 0) return;

        // kroki tylko gdy na ziemi i idzie
        if (!movement.IsGrounded || movement.HorizontalSpeed < minSpeed)
        {
            accumulated = 0f;   // reset, żeby po zatrzymaniu pierwszy krok padł od razu
            return;
        }

        // im szybciej idzie, tym szybciej narasta — krok co stepDistance przebytej drogi
        accumulated += movement.HorizontalSpeed * Time.deltaTime;

        if (accumulated >= stepDistance)
        {
            accumulated = 0f;
            PlayStep();
        }
    }

    void PlayStep()
    {
        AudioClip clip = stepClips[Random.Range(0, stepClips.Length)];
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.PlayOneShot(clip);
    }
}