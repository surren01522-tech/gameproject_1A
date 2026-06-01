using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] private int hp = 1;

    public void Initialize(int enemyHp)
    {
        hp = enemyHp;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;

        Debug.Log($"{gameObject.name} 적 체력 감소: {hp}");

        if (hp <= 0)
        {
            DestroyEnemy();
        }
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}