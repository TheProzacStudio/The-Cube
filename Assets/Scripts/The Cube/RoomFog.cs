using UnityEngine;

public class RoomFog : MonoBehaviour
{
    [SerializeField] private float fadeTime = 3f;   // dłużej = wolniej znika

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    private bool fading = false;
    private float alpha = 1f;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    void Update()
    {
        if (!fading) return;

        // płynne, wolne przygaszanie z ease
        alpha -= Time.deltaTime / fadeTime;
        if (alpha < 0f) alpha = 0f;
        float eased = Mathf.SmoothStep(0f, 1f, alpha);   // miękkie zejście

        int count = ps.GetParticles(particles);
        for (int i = 0; i < count; i++)
        {
            Color32 c = particles[i].startColor;
            c.a = (byte)(255 * eased * (baseAlpha[i] / 255f));
            particles[i].startColor = c;
        }
        ps.SetParticles(particles, count);

        // usuń dopiero gdy naprawdę niewidoczne — wtedy nie widać cięcia
        if (alpha <= 0f)
        {
            fading = false;
            ps.Clear();
            ps.Stop();
        }
    }

    private byte[] baseAlpha;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !fading)
        {
            fading = true;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            // zapamiętaj startową alfę każdej cząstki, żeby gasić proporcjonalnie
            int count = ps.GetParticles(particles);
            baseAlpha = new byte[count];
            for (int i = 0; i < count; i++)
                baseAlpha[i] = particles[i].startColor.a;
        }
    }
}