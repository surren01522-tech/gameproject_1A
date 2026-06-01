using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("Stage Data")]
    [SerializeField] private StageData currentStageData;

    [Header("Stone Prefab")]
    [SerializeField] private GameObject stonePrefab;

    [Header("Spawn Parent")]
    [SerializeField] private Transform stoneParent;
    [SerializeField] private Transform enemyParent;

    [Header("UI")]
    [SerializeField] private Text targetText;
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private GameObject failPanel;

    private int targetStoneLevel = 3;
    private int turnCount = 0;

    private bool isCleared = false;
    private bool isFailed = false;
    private Coroutine failCheckCoroutine;

    public int TargetStoneLevel => targetStoneLevel;
    public int TurnCount => turnCount;
    public bool IsCleared => isCleared;
    public bool IsFailed => isFailed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SetupStage();
        UpdateTargetUI();
        SetResultPanels(false, false);
    }

    private void SetupStage()
    {
        if (currentStageData == null)
        {
            Debug.LogError("StageManager currentStageData is not assigned.");
            return;
        }

        if (!currentStageData.ValidateStage(out string errorMessage))
        {
            Debug.LogError($"StageData setup error: {errorMessage}");
            return;
        }

        ResetStageState();
        ClearSpawnedObjects();

        targetStoneLevel = currentStageData.targetStoneLevel;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame(currentStageData);
        }

        SpawnStageStones();
        SpawnStageEnemies();
    }

    private void SpawnStageStones()
    {
        if (stonePrefab == null)
        {
            Debug.LogError("StageManager stonePrefab is not assigned.");
            return;
        }

        for (int i = 0; i < currentStageData.initialStones.Count; i++)
        {
            StageStoneData stoneData = currentStageData.initialStones[i];

            GameObject stoneObject = Instantiate(
                stonePrefab,
                stoneData.position,
                Quaternion.identity,
                stoneParent
            );

            stoneObject.name = $"StageStone_Level_{stoneData.level}";

            Stone stone = stoneObject.GetComponent<Stone>();

            if (stone == null)
            {
                Debug.LogError("stonePrefab does not have a Stone component.");
                continue;
            }

            int stoneHp = stoneData.hp > 0 ? stoneData.hp : currentStageData.defaultStoneHp;
            stone.Initialize(stoneData.level, stoneHp);

            Rigidbody2D rb = stoneObject.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }

    private void SpawnStageEnemies()
    {
        for (int i = 0; i < currentStageData.initialEnemies.Count; i++)
        {
            StageEnemyData enemyData = currentStageData.initialEnemies[i];

            if (enemyData.enemyPrefab == null)
            {
                Debug.LogWarning($"StageData enemyPrefab at index {i} is empty.");
                continue;
            }

            GameObject enemyObject = Instantiate(
                enemyData.enemyPrefab,
                enemyData.position,
                Quaternion.identity,
                enemyParent
            );

            enemyObject.name = enemyData.enemyPrefab.name;
            enemyObject.tag = "Enemy";

            Enemy enemy = enemyObject.GetComponent<Enemy>();

            if (enemy != null)
            {
                int enemyHp = enemyData.hp > 0 ? enemyData.hp : currentStageData.defaultEnemyHp;
                enemy.Initialize(enemyHp);
            }

            Rigidbody2D rb = enemyObject.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }

    public void CheckClear(int createdStoneLevel)
    {
        if (isCleared || isFailed)
        {
            return;
        }

        if (createdStoneLevel >= targetStoneLevel)
        {
            ClearStage();
        }
    }

    public void RequestFailCheck()
    {
        if (isCleared || isFailed)
        {
            return;
        }

        if (failCheckCoroutine != null)
        {
            StopCoroutine(failCheckCoroutine);
        }

        failCheckCoroutine = StartCoroutine(CheckFailAfterDelay());
    }

    public void AddTurnCount()
    {
        if (isCleared || isFailed)
        {
            return;
        }

        turnCount++;

        if (currentStageData != null &&
            currentStageData.maxTurnCount > 0 &&
            turnCount >= currentStageData.maxTurnCount)
        {
            RequestFailCheck();
        }
    }

    private IEnumerator CheckFailAfterDelay()
    {
        yield return null;

        CheckFail();
    }

    private void CheckFail()
    {
        if (isCleared || isFailed)
        {
            return;
        }

        Stone[] stones = FindObjectsByType<Stone>(FindObjectsSortMode.None);

        if (stones.Length <= 0)
        {
            FailStage();
            return;
        }

        bool hasTargetStone = false;

        for (int i = 0; i < stones.Length; i++)
        {
            if (stones[i].level >= targetStoneLevel)
            {
                hasTargetStone = true;
                break;
            }
        }

        if (hasTargetStone)
        {
            ClearStage();
            return;
        }

        bool canStillMerge = HasMergeableStones(stones);

        if (!canStillMerge)
        {
            FailStage();
        }
    }

    private bool HasMergeableStones(Stone[] stones)
    {
        for (int i = 0; i < stones.Length; i++)
        {
            for (int j = i + 1; j < stones.Length; j++)
            {
                if (stones[i].level == stones[j].level)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void ClearStage()
    {
        isCleared = true;
        Debug.Log("Stage cleared.");
        SetResultPanels(true, false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetClear();
        }
    }

    private void FailStage()
    {
        isFailed = true;
        Debug.Log("Stage failed.");
        SetResultPanels(false, true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFail();
        }
    }

    private void UpdateTargetUI()
    {
        if (targetText != null)
        {
            string targetDescription = currentStageData != null
                ? currentStageData.TargetDescription
                : $"Create a level {targetStoneLevel} stone";

            targetText.text = $"Goal: {targetDescription}";
        }
    }

    private void ResetStageState()
    {
        isCleared = false;
        isFailed = false;
        turnCount = 0;
    }

    private void SetResultPanels(bool showClear, bool showFail)
    {
        if (clearPanel != null)
        {
            clearPanel.SetActive(showClear);
        }

        if (failPanel != null)
        {
            failPanel.SetActive(showFail);
        }
    }

    private void ClearSpawnedObjects()
    {
        ClearChildren(stoneParent);
        ClearChildren(enemyParent);
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}
