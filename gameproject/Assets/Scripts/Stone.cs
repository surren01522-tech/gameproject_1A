using UnityEngine;

public class Stone : MonoBehaviour
{
    [Header("Stone Data")]
    public int level = 1;
    public int hp = 3;

    [Header("State")]
    public bool isMerging = false;

    [Header("Sprite")]
    public SpriteRenderer StoneSprite;
    public Sprite[] StoneType;

    private void Start()
    {
        UpdateSprite();
    }

    public void Initialize(int stoneLevel, int stoneHp)
    {
        level = stoneLevel;
        hp = stoneHp;
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
            Debug.LogWarning($"StoneType 배열에 level {level}에 해당하는 스프라이트가 없습니다.");
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
            TakeDamage(1);
            return;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(1);

            Enemy enemy = collision.gameObject.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(1);
            }

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
        if (isMerging)
        {
            return;
        }

        hp -= damage;

        Debug.Log($"{gameObject.name} 내구도 감소: {hp}");

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