using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] private int maxHp = 8;
    [SerializeField] private int currentHp = 8;

    [Header("Collision Damage")]
    [SerializeField] private int wallCollisionDamage = 1;
    [SerializeField] private int enemyCollisionDamageToPlayer = 1;
    [SerializeField] private int playerDamageToEnemy = 1;

    [Header("Damage Rules")]
    [SerializeField] private float minimumDamageSpeed = 0.08f;
    [SerializeField] private float damageCooldown = 0.15f;

    [Header("Health Bar")]
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0f, -0.82f, 0f);
    [SerializeField] private float healthBarWidth = 0.9f;
    [SerializeField] private float healthBarHeight = 0.08f;

    private Rigidbody2D rb;
    private HealthBar2D healthBar;
    private bool isDead = false;
    private float nextDamageTime = 0f;

    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public bool IsDead => isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ResetHealth();
        SetupHealthBar();
    }

    private void OnEnable()
    {
        ResetHealth();
        UpdateHealthBar();
    }

    private void Start()
    {
        ResetHealth();
        UpdateHealthBar();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollisionDamage(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollisionDamage(collision);
    }

    private void HandleCollisionDamage(Collision2D collision)
    {
        if (!CanTakeCollisionDamage(collision))
        {
            return;
        }

        Enemy enemy = collision.gameObject.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            TakeDamage(enemyCollisionDamageToPlayer);
            enemy.TakeDamage(playerDamageToEnemy);
            return;
        }

        if (IsWallCollision(collision.gameObject))
        {
            TakeDamage(wallCollisionDamage);
        }
    }

    private bool CanTakeCollisionDamage(Collision2D collision)
    {
        if (isDead || Time.time < nextDamageTime)
        {
            return false;
        }

        float playerSpeed = rb != null ? rb.linearVelocity.magnitude : 0f;
        float relativeSpeed = collision.relativeVelocity.magnitude;

        return playerSpeed >= minimumDamageSpeed || relativeSpeed >= minimumDamageSpeed;
    }

    private bool IsWallCollision(GameObject collisionObject)
    {
        if (collisionObject.CompareTag("Wall"))
        {
            return true;
        }

        if (collisionObject.GetComponentInParent<Enemy>() != null)
        {
            return false;
        }

        if (collisionObject.GetComponentInParent<Stone>() != null)
        {
            return false;
        }

        Rigidbody2D otherBody = collisionObject.GetComponentInParent<Rigidbody2D>();

        if (otherBody == null || otherBody.bodyType == RigidbodyType2D.Static)
        {
            return true;
        }

        Transform current = collisionObject.transform;

        while (current != null)
        {
            string objectName = current.name;

            if (objectName.Contains("Wall") || objectName.Contains("Walls") || objectName.Contains("Border") || objectName.Contains("Boundary") || objectName.Contains("Line") || objectName.Contains("Square"))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void ResetHealth()
    {
        maxHp = 8;
        currentHp = maxHp;
        isDead = false;
        nextDamageTime = 0f;
    }

    private void SetupHealthBar()
    {
        healthBar = GetComponent<HealthBar2D>();

        if (healthBar == null)
        {
            healthBar = gameObject.AddComponent<HealthBar2D>();
        }

        healthBar.Setup(healthBarOffset, healthBarWidth, healthBarHeight, new Color(0.25f, 0.9f, 0.25f, 1f), 90);
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.SetValue(currentHp, maxHp);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        currentHp = Mathf.Max(0, currentHp - damage);
        nextDamageTime = Time.time + damageCooldown;
        UpdateHealthBar();

        Debug.Log($"{gameObject.name} player hp: {currentHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        if (StageManager.Instance != null)
        {
            StageManager.Instance.RequestFailCheck();
        }
    }
}