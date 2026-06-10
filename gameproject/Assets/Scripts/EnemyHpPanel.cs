using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpPanel : MonoBehaviour
{
    public static EnemyHpPanel Instance { get; private set; }

    [Header("Panel")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button toggleButton;

    [Header("Panel Move")]
    [SerializeField] private float openX = 0f;
    [SerializeField] private float closeX = -260f;
    [SerializeField] private float moveSpeed = 10f;

    [Header("UI")]
    [SerializeField] private Transform enemyHpListParent;
    [SerializeField] private EnemyHpItem enemyHpItemPrefab;

    private readonly List<EnemyHpItem> enemyHpItems = new List<EnemyHpItem>();

    private bool isOpen = false;
    private float targetPanelX;

    private Coroutine refreshCoroutine;

    private void Awake()
    {
        Instance = this;

        if (panel == null)
        {
            panel = GetComponent<RectTransform>();
        }

        targetPanelX = openX;
    }

    private void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(TogglePanel);
            toggleButton.onClick.AddListener(TogglePanel);
        }

        RequestRefresh();
    }

    private void Update()
    {
        MovePanel();
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
        SoundManager_1.Instance.PlaySfx(SfxType.ButtonClick);
        SoundManager_1.Instance.PlaySfx(SfxType.PanelOpen);
        SoundManager_1.Instance.PlaySfx(SfxType.PanelClose);

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

    public void RequestRefresh()
    {
        if (refreshCoroutine != null)
        {
            return;
        }

        refreshCoroutine = StartCoroutine(RefreshNextFrame());
    }

    private IEnumerator RefreshNextFrame()
    {
        yield return null;

        Refresh();

        refreshCoroutine = null;
    }

    public void Refresh()
    {
        ClearItems();

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null)
            {
                continue;
            }

            CreateEnemyHpItem(enemies[i]);
        }
    }

    private void CreateEnemyHpItem(Enemy enemy)
    {
        if (enemyHpListParent == null)
        {
            Debug.LogError("EnemyHpPanel에 enemyHpListParent가 연결되지 않았습니다.");
            return;
        }

        if (enemyHpItemPrefab == null)
        {
            Debug.LogError("EnemyHpPanel에 enemyHpItemPrefab이 연결되지 않았습니다.");
            return;
        }

        EnemyHpItem item = Instantiate(enemyHpItemPrefab, enemyHpListParent);
        item.Setup(enemy);

        enemyHpItems.Add(item);
    }

    private void ClearItems()
    {
        for (int i = 0; i < enemyHpItems.Count; i++)
        {
            if (enemyHpItems[i] != null)
            {
                Destroy(enemyHpItems[i].gameObject);
            }
        }

        enemyHpItems.Clear();
    }
}