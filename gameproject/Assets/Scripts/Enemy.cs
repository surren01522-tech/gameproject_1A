using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] private int hp = 1;

    [Header("UI Data")]
    [SerializeField] private Sprite enemyIcon;

    public int Hp => hp;

    public Sprite EnemyIcon
    {
        get
        {
            if (enemyIcon != null)
            {
                return enemyIcon;
            }

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                return spriteRenderer.sprite;
            }

            SpriteRenderer childSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (childSpriteRenderer != null)
            {
                return childSpriteRenderer.sprite;
            }

            return null;
        }
    }

    private void OnEnable()
    {
        if (EnemyHpPanel.Instance != null)
        {
            EnemyHpPanel.Instance.RequestRefresh();
        }
    }

    private void OnDestroy()
    {
        if (EnemyHpPanel.Instance != null)
        {
            EnemyHpPanel.Instance.RequestRefresh();
        }
    }

    public void Initialize(int enemyHp)
    {
        hp = enemyHp;

        if (EnemyHpPanel.Instance != null)
        {
            EnemyHpPanel.Instance.RequestRefresh();
        }
    }

    public void TakeDamage(int damage)
    {
        SoundManager_1.Instance.PlaySfx(SfxType.HitEnemy);
        hp -= damage;

        Debug.Log($"{gameObject.name} 적 체력 감소: {hp}");

        if (EnemyHpPanel.Instance != null)
        {
            EnemyHpPanel.Instance.RequestRefresh();
        }

        if (hp <= 0)
        {
            DestroyEnemy();
        }
    }

    private void DestroyEnemy()
    {
        SoundManager_1.Instance.PlaySfx(SfxType.EnemyDead);
        Destroy(gameObject);
    }
}