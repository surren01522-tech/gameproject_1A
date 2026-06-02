using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageStatusPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button toggleButton;

    [Header("Panel Move")]
    [SerializeField] private float openX = 0f;
    [SerializeField] private float closeX = 260f;
    [SerializeField] private float moveSpeed = 10f;

    [Header("Remaining Stones UI")]
    [SerializeField] private Transform remainingStoneParent;
    [SerializeField] private Image remainingStoneIconPrefab;

    [Header("Target UI")]
    [SerializeField] private Image targetStoneIcon;
    [SerializeField] private Text targetText;

    [Header("Stone Sprite")]
    [SerializeField] private Sprite[] stoneSprites;

    [Header("Refresh")]
    [SerializeField] private float refreshInterval = 0.2f;

    private bool isOpen = true;
    private float targetPanelX;

    private float refreshTimer = 0f;

    private readonly List<Image> createdIcons = new List<Image>();

    private void Start()
    {
        targetPanelX = openX;

        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(TogglePanel);
            toggleButton.onClick.AddListener(TogglePanel);
        }

        RefreshAll();
    }

    private void Update()
    {
        MovePanel();

        refreshTimer += Time.deltaTime;

        if (refreshTimer >= refreshInterval)
        {
            refreshTimer = 0f;
            RefreshRemainingStones();
        }
    }

    private void MovePanel()
    {
        if (panel == null)
        {
            return;
        }

        Vector2 currentPosition = panel.anchoredPosition;
        Vector2 targetPosition = new Vector2(targetPanelX, currentPosition.y);

        panel.anchoredPosition = Vector2.Lerp(
            currentPosition,
            targetPosition,
            Time.deltaTime * moveSpeed
        );
    }

    private void TogglePanel()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            targetPanelX = openX;
        }
        else
        {
            targetPanelX = closeX;
        }
    }

    private void RefreshAll()
    {
        RefreshTarget();
        RefreshRemainingStones();
    }

    private void RefreshTarget()
    {
        if (StageManager.Instance == null)
        {
            return;
        }

        int targetLevel = StageManager.Instance.TargetStoneLevel;

        if (targetText != null)
        {
            targetText.text = $"Lv.{targetLevel} 돌 만들기";
        }

        if (targetStoneIcon != null)
        {
            targetStoneIcon.sprite = GetStoneSprite(targetLevel);
        }
    }

    private void RefreshRemainingStones()
    {
        ClearIcons();

        Stone[] stones = FindObjectsByType<Stone>(FindObjectsSortMode.None);

        for (int i = 0; i < stones.Length; i++)
        {
            Stone stone = stones[i];

            if (stone == null)
            {
                continue;
            }

            CreateStoneIcon(stone.level);
        }
    }

    private void CreateStoneIcon(int stoneLevel)
    {
        if (remainingStoneParent == null)
        {
            return;
        }

        if (remainingStoneIconPrefab == null)
        {
            return;
        }

        Image icon = Instantiate(remainingStoneIconPrefab, remainingStoneParent);

        icon.sprite = GetStoneSprite(stoneLevel);
        icon.gameObject.SetActive(true);

        createdIcons.Add(icon);
    }

    private void ClearIcons()
    {
        for (int i = 0; i < createdIcons.Count; i++)
        {
            if (createdIcons[i] != null)
            {
                Destroy(createdIcons[i].gameObject);
            }
        }

        createdIcons.Clear();
    }

    private Sprite GetStoneSprite(int level)
    {
        if (stoneSprites == null || stoneSprites.Length <= 0)
        {
            return null;
        }

        int index = level - 1;

        if (index < 0 || index >= stoneSprites.Length)
        {
            return null;
        }

        return stoneSprites[index];
    }
}