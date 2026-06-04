using UnityEngine;

public class PlayerHeart : MonoBehaviour
{
    [Header("Heart")]
    [SerializeField] private int maxHeart = 3;
    [SerializeField] private int currentHeart = 3;

    public int MaxHeart => maxHeart;
    public int CurrentHeart => currentHeart;

    private bool isDead = false;

    private void Start()
    {
        currentHeart = maxHeart;

        if (HeartPanel.Instance != null)
        {
            HeartPanel.Instance.Setup(maxHeart, currentHeart);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        if (damage <= 0)
        {
            return;
        }

        currentHeart -= damage;
        currentHeart = Mathf.Clamp(currentHeart, 0, maxHeart);

        Debug.Log($"플레이어 하트 감소: {currentHeart}");

        if (HeartPanel.Instance != null)
        {
            HeartPanel.Instance.SetHeart(currentHeart);
        }

        if (currentHeart <= 0)
        {
            Die();
        }
    }

    public void Heal(int heal)
    {
        if (isDead)
        {
            return;
        }

        if (heal <= 0)
        {
            return;
        }

        currentHeart += heal;
        currentHeart = Mathf.Clamp(currentHeart, 0, maxHeart);

        Debug.Log($"플레이어 하트 회복: {currentHeart}");

        if (HeartPanel.Instance != null)
        {
            HeartPanel.Instance.SetHeart(currentHeart);
        }
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("플레이어 사망 - 스테이지 실패");

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(SfxType.Fail);
        }

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayEffect(EffectType.Fail, transform.position);
        }

        if (StageManager.Instance != null)
        {
            StageManager.Instance.ForceFail();
        }
        else if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFail();
        }
    }
}