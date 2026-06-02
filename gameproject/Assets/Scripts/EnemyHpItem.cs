using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHpItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image enemyIconImage;
    [SerializeField] private TextMeshProUGUI hpText;

    private Enemy targetEnemy;

    public void Setup(Enemy enemy)
    {
        targetEnemy = enemy;

        Refresh();
    }

    public void Refresh()
    {
        if (targetEnemy == null)
        {
            Destroy(gameObject);
            return;
        }

        if (hpText != null)
        {
            hpText.text = $"HP {targetEnemy.Hp}";
        }

        if (enemyIconImage != null)
        {
            enemyIconImage.sprite = targetEnemy.EnemyIcon;
        }
    }
}