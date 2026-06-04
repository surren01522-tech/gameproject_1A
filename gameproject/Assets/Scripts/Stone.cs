using UnityEngine;

public class Stone : MonoBehaviour
{
    [Header("Stone Data")]
    public int level = 1;

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
        // stoneHp는 StageData 호환용으로 받아두지만,
        // 이제 돌은 데미지를 받지 않으므로 사용하지 않음
        level = stoneLevel;
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

        // 돌은 이제 벽에 부딪혀도 데미지를 받지 않음
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySfx(SfxType.HitWall);
            }

            if (EffectManager.Instance != null && collision.contactCount > 0)
            {
                Vector3 hitPosition = collision.GetContact(0).point;
                EffectManager.Instance.PlayEffect(EffectType.HitWall, hitPosition);
            }

            return;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySfx(SfxType.HitEnemy);
            }

            if (EffectManager.Instance != null && collision.contactCount > 0)
            {
                Vector3 hitPosition = collision.GetContact(0).point;
                EffectManager.Instance.PlayEffect(EffectType.HitEnemy, hitPosition);
            }

            Enemy enemy = collision.gameObject.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(1);
            }

            return;
        }

        // 같은 레벨 돌끼리만 합성
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
}