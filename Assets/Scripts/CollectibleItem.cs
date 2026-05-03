using UnityEngine;

// Attach to every collectible object in the scene.
// hookHead must be tagged "Hook" with a trigger Collider2D.
public class CollectibleItem : MonoBehaviour
{
    public string itemName;
    public bool   isTarget;

    [Header("Visuals (auto-set by ItemSpawner)")]
    public SpriteRenderer glowRenderer;       // inner ring
    public SpriteRenderer glowRendererOuter;  // outer halo
    public ParticleSystem sparkleParticles;

    float _pulseT;

    void Start()
    {
        if (glowRenderer      != null) glowRenderer.gameObject.SetActive(isTarget);
        if (glowRendererOuter != null) glowRendererOuter.gameObject.SetActive(isTarget);
        if (sparkleParticles  != null && isTarget) sparkleParticles.Play();
    }

    void Update()
    {
        if (!isTarget) return;
        _pulseT += Time.deltaTime * 2.5f;
        float pulse = 0.5f + 0.5f * Mathf.Sin(_pulseT);

        if (glowRenderer != null)
        {
            Color c = GameColors.MeterGreen;
            c.a = 0.45f + 0.40f * pulse;
            glowRenderer.color = c;
        }
        if (glowRendererOuter != null)
        {
            Color c = GameColors.Butter;
            c.a = 0.12f + 0.20f * pulse;
            glowRendererOuter.color = c;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Hook")) return;
        var hook = other.GetComponentInParent<HookController>();
        hook?.GrabItem(this);
    }
}
