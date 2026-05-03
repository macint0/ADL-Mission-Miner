using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Continuously spawns items every spawnInterval seconds.
// Caps at maxItems — evicts the oldest when full.
public class ItemSpawner : MonoBehaviour
{
    [Header("Continuous Spawn")]
    public int   maxItems      = 10;
    public float spawnInterval = 10f;
    [Range(0f, 1f)]
    public float targetRatio   = 0.7f;   // probability each new item is a target

    [Header("Shelf Layout")]
    public float shelfMinX  =  -8f;
    public float shelfMaxX  =   8f;
    public float shelfMinY  =  -4f;
    public float shelfMaxY  =  2.5f;
    public float minSpacing =  3.2f;

    [Header("Item Visuals")]
    public float itemScale    = 0.5f;
    [Tooltip("Hitbox radius as a fraction of the sprite visual size. 1 = exact sprite edge, 0.6 = tighter.")]
    [Range(0.3f, 1.5f)]
    public float hitboxScale  = 0.65f;

    [Header("Debug")]
    public bool showHitboxes = false;

    [Header("Prefabs / Refs")]
    public GameObject     itemPrefab;
    public ParticleSystem sparklePrefab;

    // Edible = target (glows, gives points)
    static readonly string[] edibleItems =
    {
        "apple", "banana", "bread", "carrot",
        "cheese", "cookie", "donut", "egg",
        "grapes", "sandwich"
    };

    // Non-edible = distractor (no glow, costs points)
    static readonly string[] badItems =
    {
        "battery", "bleach", "rock", "screw", "sock",
        "bag", "comb", "hairbrush", "glasses", "shoe",
        "keys", "soap", "toothbrush", "toothpaste",
        "cup", "bottle", "medication"
    };

    readonly List<GameObject> _active      = new List<GameObject>();
    readonly List<Vector3>    usedPositions = new List<Vector3>();
    Coroutine _loop;

    // ------------------------------------------------------------------ public API

    public void StartContinuousSpawning()
    {
        _active.Clear();
        usedPositions.Clear();

        // Fill the screen immediately
        for (int i = 0; i < maxItems; i++)
            SpawnOne();

        _loop = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (_loop != null) StopCoroutine(_loop);
        _loop = null;
    }

    // ------------------------------------------------------------------ internal

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnOne();
        }
    }

    void SpawnOne()
    {
        // Drop destroyed / collected entries
        _active.RemoveAll(g => g == null || !g.activeSelf);

        // Evict oldest when at cap
        if (_active.Count >= maxItems)
        {
            Destroy(_active[0]);
            _active.RemoveAt(0);
        }

        // Rebuild occupied positions from what is still alive
        usedPositions.Clear();
        foreach (var g in _active)
            if (g != null) usedPositions.Add(g.transform.position);

        bool   isTarget = Random.value < targetRatio;
        string[] pool   = isTarget ? edibleItems : badItems;
        string id       = pool[Random.Range(0, pool.Length)];

        var go = SpawnItem(id, isTarget);
        if (go != null) _active.Add(go);
    }

    GameObject SpawnItem(string id, bool isTarget)
    {
        Sprite spr = Resources.Load<Sprite>("Items/" + id);
        if (spr == null)
            Debug.LogWarning($"Sprite not found: Resources/Items/{id}");

        var go = Instantiate(itemPrefab, FindFreePosition(), Quaternion.identity);
        go.name = id;
        go.transform.localScale = Vector3.one * itemScale;
        go.SetActive(true);

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null && spr != null) sr.sprite = spr;

        // Collider radius: sprite half-size in local space, scaled by hitboxScale.
        var col = go.GetComponent<CircleCollider2D>();
        if (col != null) col.radius = (512f / 100f / 2f) / itemScale * hitboxScale;

        if (showHitboxes && col != null)
            DrawHitboxCircle(go, col.radius);

        var item = go.GetComponent<CollectibleItem>();
        if (item == null) return go;

        item.itemName = id;
        item.isTarget = isTarget;

        if (!isTarget) return go;

        int baseSortOrder = sr != null ? sr.sortingOrder : 3;

        // Outer halo: 2.5× item size  →  local scale = 4 × 2.5 = 10
        item.glowRendererOuter = MakeGlowLayer(go, "GlowOuter", 10f,
            new Color(1f, 0.91f, 0.66f, 0.28f), baseSortOrder - 2);

        // Inner ring: 1.5× item size  →  local scale = 4 × 1.5 = 6
        item.glowRenderer = MakeGlowLayer(go, "GlowInner", 6f,
            new Color(0.91f, 0.76f, 0.35f, 0.60f), baseSortOrder - 1);

        if (sparklePrefab != null)
            item.sparkleParticles = Instantiate(sparklePrefab, go.transform);

        return go;
    }

    // ------------------------------------------------------------------ helpers

    SpriteRenderer MakeGlowLayer(GameObject parent, string name, float scale, Color color, int sortOrder)
    {
        var glowGo = new GameObject(name);
        glowGo.transform.SetParent(parent.transform, false);
        glowGo.transform.localScale = Vector3.one * scale;
        var sr = glowGo.AddComponent<SpriteRenderer>();
        sr.sprite       = GetGlowSprite();
        sr.color        = color;
        sr.sortingOrder = sortOrder;
        return sr;
    }

    static Sprite _glowSprite;
    static Sprite GetGlowSprite()
    {
        if (_glowSprite != null) return _glowSprite;
        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float center = size / 2f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
            float t    = Mathf.Clamp01(1f - dist / center);
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, t * t * t));
        }
        tex.Apply();
        _glowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return _glowSprite;
    }

    void DrawHitboxCircle(GameObject parent, float localRadius)
    {
        var go = new GameObject("HitboxDebug");
        go.transform.SetParent(parent.transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace   = false;
        lr.loop            = true;
        lr.widthMultiplier = 0.04f;
        lr.material        = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = new Color(1f, 0f, 0f, 0.9f);
        lr.sortingOrder    = 99;
        const int seg = 40;
        lr.positionCount = seg;
        for (int i = 0; i < seg; i++)
        {
            float a = i / (float)seg * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * localRadius, Mathf.Sin(a) * localRadius, -0.1f));
        }
    }

    Vector3 FindFreePosition()
    {
        for (int attempt = 0; attempt < 200; attempt++)
        {
            float x = Random.Range(shelfMinX, shelfMaxX);
            float y = Random.Range(shelfMinY, shelfMaxY);
            var candidate = new Vector3(x, y, 0);
            if (!usedPositions.Exists(p => Vector3.Distance(p, candidate) < minSpacing))
            {
                usedPositions.Add(candidate);
                return candidate;
            }
        }
        var fallback = new Vector3(Random.Range(shelfMinX, shelfMaxX), shelfMinY, 0);
        usedPositions.Add(fallback);
        return fallback;
    }
}
