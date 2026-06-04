using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartPanel : MonoBehaviour
{
    public static HeartPanel Instance { get; private set; }

    [Header("Heart UI")]
    [SerializeField] private Transform heartParent;
    [SerializeField] private Image heartIconPrefab;

    [Header("Heart Sprite")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    private readonly List<Image> heartIcons = new List<Image>();

    private int maxHeart = 3;
    private int currentHeart = 3;

    public int MaxHeart => maxHeart;
    public int CurrentHeart => currentHeart;

    private void Awake()
    {
        Instance = this;
    }

    public void Setup(int maxHeartValue, int currentHeartValue)
    {
        maxHeart = maxHeartValue;
        currentHeart = Mathf.Clamp(currentHeartValue, 0, maxHeart);

        Refresh();
    }

    public void SetHeart(int value)
    {
        currentHeart = Mathf.Clamp(value, 0, maxHeart);

        Refresh();
    }

    private void Refresh()
    {
        ClearHearts();

        if (heartParent == null)
        {
            Debug.LogError("HeartPanel에 heartParent가 연결되지 않았습니다.");
            return;
        }

        if (heartIconPrefab == null)
        {
            Debug.LogError("HeartPanel에 heartIconPrefab이 연결되지 않았습니다.");
            return;
        }

        for (int i = 0; i < maxHeart; i++)
        {
            Image heartIcon = Instantiate(heartIconPrefab, heartParent);

            if (i < currentHeart)
            {
                heartIcon.sprite = fullHeartSprite;
            }
            else
            {
                heartIcon.sprite = emptyHeartSprite;
            }

            heartIcon.gameObject.SetActive(true);
            heartIcons.Add(heartIcon);
        }
    }

    private void ClearHearts()
    {
        for (int i = 0; i < heartIcons.Count; i++)
        {
            if (heartIcons[i] != null)
            {
                Destroy(heartIcons[i].gameObject);
            }
        }

        heartIcons.Clear();
    }
}