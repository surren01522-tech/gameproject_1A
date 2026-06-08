using UnityEngine;

public class HealthBar2D : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, -0.75f, 0f);
    [SerializeField] private float width = 0.75f;
    [SerializeField] private float height = 0.08f;
    [SerializeField] private int sortingOrder = 50;

    [Header("Color")]
    [SerializeField] private Color fillColor = new Color(0.25f, 0.9f, 0.25f, 1f);
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);

    private Transform barRoot;
    private SpriteRenderer backgroundRenderer;
    private SpriteRenderer fillRenderer;
    private float currentRatio = 1f;

    private static Sprite squareSprite;

    public void Setup(Vector3 offset, float barWidth, float barHeight, Color fill, int order)
    {
        worldOffset = offset;
        width = barWidth;
        height = barHeight;
        fillColor = fill;
        sortingOrder = order;

        EnsureBarCreated();
        ApplyVisuals();
        UpdateTransform();
        SetRatio(1f);
    }

    public void SetValue(int currentHp, int maxHp)
    {
        float ratio = maxHp <= 0 ? 0f : Mathf.Clamp01((float)currentHp / maxHp);
        SetRatio(ratio);
    }

    private void Awake()
    {
        EnsureBarCreated();
        ApplyVisuals();
        SetRatio(currentRatio);
    }

    private void LateUpdate()
    {
        UpdateTransform();
    }

    private void EnsureBarCreated()
    {
        if (barRoot != null)
        {
            return;
        }

        if (squareSprite == null)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            squareSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        GameObject rootObject = new GameObject("HealthBar");
        rootObject.transform.SetParent(transform, false);
        barRoot = rootObject.transform;

        backgroundRenderer = CreatePart("Background", backgroundColor, sortingOrder);
        fillRenderer = CreatePart("Fill", fillColor, sortingOrder + 1);
    }

    private SpriteRenderer CreatePart(string partName, Color color, int order)
    {
        GameObject partObject = new GameObject(partName);
        partObject.transform.SetParent(barRoot, false);

        SpriteRenderer renderer = partObject.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.color = color;
        renderer.sortingOrder = order;

        return renderer;
    }

    private void ApplyVisuals()
    {
        if (backgroundRenderer != null)
        {
            backgroundRenderer.color = backgroundColor;
            backgroundRenderer.sortingOrder = sortingOrder;
            backgroundRenderer.transform.localScale = new Vector3(width, height, 1f);
            backgroundRenderer.transform.localPosition = Vector3.zero;
        }

        if (fillRenderer != null)
        {
            fillRenderer.color = fillColor;
            fillRenderer.sortingOrder = sortingOrder + 1;
        }
    }

    private void SetRatio(float ratio)
    {
        currentRatio = Mathf.Clamp01(ratio);

        if (fillRenderer == null)
        {
            return;
        }

        float fillWidth = width * currentRatio;
        fillRenderer.transform.localScale = new Vector3(fillWidth, height, 1f);
        fillRenderer.transform.localPosition = new Vector3((fillWidth - width) * 0.5f, 0f, -0.01f);
    }

    private void UpdateTransform()
    {
        if (barRoot == null)
        {
            return;
        }

        barRoot.position = transform.position + worldOffset;
        barRoot.rotation = Quaternion.identity;
        barRoot.localScale = Vector3.one;
    }
}