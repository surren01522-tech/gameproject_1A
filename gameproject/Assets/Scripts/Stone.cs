using UnityEngine;

public class Stone : MonoBehaviour
{
    [Header("Stone Data")]
    public int level = 1;
    public int hp = 3;

    [Header("Collision Damage")]
    [SerializeField] private int wallCollisionDamage = 1;

    [Header("State")]
    public bool isMerging = false;

    [Header("Sprite")]
    public SpriteRenderer StoneSprite;
    public Sprite[] StoneType;

    private int maxHp = 3;

    public int CurrentHp => hp;
    public int MaxHp => maxHp;

    private void Awake()
    {
        maxHp = Mathf.Max(1, hp);
    }

    private void Start()
    {
        UpdateSprite();
    }

    public void Initialize(int stoneLevel, int stoneHp)
    {
        level = stoneLevel;
        hp = stoneHp;
        maxHp = Mathf.Max(1, stoneHp);
        isMerging = false;

        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (StoneSprite == null)
        {
            return;
        }

        if (StoneType == null || StoneType.Length <= 0)
        {
            return;
        }

        int spriteIndex = level - 1;

        if (spriteIndex < 0 || spriteIndex >= StoneType.Length)
        {
            Debug.LogWarning($"StoneType sprite is missing for level {level}.");
            return;
        }

        StoneSprite.sprite = StoneType[spriteIndex];
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isMerging)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            TakeDamage(wallCollisionDamage);
            return;
        }

        if (collision.gameObject.CompareTag("Stone"))
        {
            Stone otherStone = collision.gameObject.GetComponent<Stone>();

            if (otherStone == null)
            {
                return;
            }

            if (otherStone.isMerging)
            {
                return;
            }

            if (level == otherStone.level)
            {
                MergeManager.Instance.TryMerge(this, otherStone);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isMerging || damage <= 0)
        {
            return;
        }

        hp -= damage;

        Debug.Log($"{gameObject.name} stone hp: {hp}");

        if (hp <= 0)
        {
            DestroyStone();
        }
    }

    private void DestroyStone()
    {
        Destroy(gameObject);

        if (StageManager.Instance != null)
        {
            StageManager.Instance.RequestFailCheck();
        }
    }
}
