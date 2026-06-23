using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BlockBreakEffect : MonoBehaviour
{
    private const int DefaultSortingOrder = 12;

    [SerializeField] private float duration = 0.3f;
    [SerializeField] private float startScale = 0.8f;
    [SerializeField] private float endScale = 1.35f;
    [SerializeField] private Color effectColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private SpriteRenderer spriteRenderer;

    private static Sprite fallbackSprite;

    private float elapsed;
    private Vector3 baseScale = Vector3.one;
    private Color startColor;

    private void Awake()
    {
        EnsureRenderer();
        baseScale = transform.localScale;
        startColor = effectColor;
        ApplyFrame(0f);
    }

    private void Update()
    {
        float safeDuration = Mathf.Max(0.01f, duration);
        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / safeDuration);

        ApplyFrame(progress);

        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }

    public void Configure(Color color, float effectDuration, float effectStartScale, float effectEndScale, int sortingOrder)
    {
        duration = Mathf.Max(0.01f, effectDuration);
        startScale = Mathf.Max(0.01f, effectStartScale);
        endScale = Mathf.Max(startScale, effectEndScale);
        effectColor = color;
        startColor = color;
        baseScale = transform.localScale;

        EnsureRenderer();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }

        elapsed = 0f;
        ApplyFrame(0f);
    }

    private void EnsureRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            return;
        }

        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = GetFallbackSprite();
        }

        if (spriteRenderer.sortingOrder == 0)
        {
            spriteRenderer.sortingOrder = DefaultSortingOrder;
        }
    }

    private void ApplyFrame(float progress)
    {
        float scale = Mathf.Lerp(startScale, endScale, progress);
        transform.localScale = baseScale * scale;

        if (spriteRenderer == null)
        {
            return;
        }

        Color color = startColor;
        color.a *= 1f - progress;
        spriteRenderer.color = color;
    }

    private static Sprite GetFallbackSprite()
    {
        if (fallbackSprite != null)
        {
            return fallbackSprite;
        }

        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.45f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius + 1f - distance);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        fallbackSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        fallbackSprite.name = "Runtime_BlockBreakEffectSprite";
        return fallbackSprite;
    }

    private void OnValidate()
    {
        duration = Mathf.Max(0.01f, duration);
        startScale = Mathf.Max(0.01f, startScale);
        endScale = Mathf.Max(startScale, endScale);
    }
}
