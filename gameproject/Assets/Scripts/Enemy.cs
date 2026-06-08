using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] private int hp = 3;

    [Header("Health Bar")]
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0f, -0.72f, 0f);
    [SerializeField] private float healthBarWidth = 0.8f;
    [SerializeField] private float healthBarHeight = 0.08f;

    private int maxHp = 3;
    private bool isDead = false;
    private HealthBar2D healthBar;

    public int CurrentHp => hp;
    public int MaxHp => maxHp;
    public bool IsDead => isDead;

    private void Awake()
    {
        hp = Mathf.Max(1, hp);
        maxHp = hp;
        isDead = false;
        SetupHealthBar();
    }

    private void Start()
    {
        UpdateHealthBar();
    }

    public void Initialize(int enemyHp)
    {
        hp = Mathf.Max(1, enemyHp);
        maxHp = hp;
        isDead = false;

        UpdateHealthBar();
    }

    private void SetupHealthBar()
    {
        healthBar = GetComponent<HealthBar2D>();

        if (healthBar == null)
        {
            healthBar = gameObject.AddComponent<HealthBar2D>();
        }

        healthBar.Setup(healthBarOffset, healthBarWidth, healthBarHeight, new Color(0.95f, 0.25f, 0.2f, 1f), 80);
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.SetValue(hp, maxHp);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        hp -= damage;
        UpdateHealthBar();

        Debug.Log($"{gameObject.name} enemy hp: {hp}");

        if (hp <= 0)
        {
            DestroyEnemy();
        }
    }

    private void DestroyEnemy()
    {
        isDead = true;
        Destroy(gameObject);
    }
}
