using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageEffect : MonoBehaviour
{
    [Header("Player Sprite")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [Header("Portrait UI")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Sprite normalPortrait;
    [SerializeField] private Sprite hitPortrait;

    [Header("Effect Setting")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float effectDuration = 0.15f;

    private Color originalColor;
    private Coroutine damageEffectCoroutine;

    private void Awake()
    {
        if (playerSpriteRenderer != null)
        {
            originalColor = playerSpriteRenderer.color;
        }

        if (portraitImage != null && normalPortrait != null)
        {
            portraitImage.sprite = normalPortrait;
        }
    }

    public void PlayDamageEffect()
    {
        if (damageEffectCoroutine != null)
        {
            StopCoroutine(damageEffectCoroutine);
        }

        damageEffectCoroutine = StartCoroutine(DamageEffectRoutine());
    }

    private IEnumerator DamageEffectRoutine()
    {
        if (portraitImage != null && hitPortrait != null)
        {
            portraitImage.sprite = hitPortrait;
        }

        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = hitColor;
        }

        yield return new WaitForSeconds(effectDuration);

        if (portraitImage != null && normalPortrait != null)
        {
            portraitImage.sprite = normalPortrait;
        }

        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = originalColor;
        }

        damageEffectCoroutine = null;
    }
}