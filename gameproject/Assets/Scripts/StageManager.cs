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
    [SerializeField] private Text stageNameText;
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private GameObject failPanel;

    private int targetStoneLevel = 3;

    private bool isCleared = false;
    private bool isFailed = false;
    private Coroutine failCheckCoroutine;

    public int TargetStoneLevel => targetStoneLevel;
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

        ApplySelectedStageData();
    }

    private void Start()
    {
        SetupStage();

        UpdateStageUI();

        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }

        if (failPanel != null)
        {
            failPanel.SetActive(false);
        }
    }

    private void ApplySelectedStageData()
    {
        if (SelectedStageData.CurrentStageData != null)
        {
            currentStageData = SelectedStageData.CurrentStageData;
        }
    }

    private void SetupStage()
    {
        if (currentStageData == null)
        {
            Debug.LogError("StageManager에 currentStageData가 없습니다. 스테이지 선택 또는 인스펙터 연결을 확인하세요.");
            return;
        }

        targetStoneLevel = currentStageData.targetStoneLevel;

        SpawnStageStones();
        SpawnStageEnemies();
    }

    private void SpawnStageStones()
    {
        if (stonePrefab == null)
        {
            Debug.LogError("StageManager에 stonePrefab이 연결되지 않았습니다.");
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
                Debug.LogError("stonePrefab에 Stone.cs가 없습니다.");
                continue;
            }

            stone.Initialize(stoneData.level, stoneData.hp);

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
                Debug.LogWarning($"StageData의 {i}번째 enemyPrefab이 비어 있습니다.");
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
                enemy.Initialize(enemyData.hp);
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
        SoundManager_1.Instance.PlaySfx(SfxType.Clear);

        isCleared = true;

        Debug.Log("스테이지 클리어");

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetClear();
        }
    }

    private void FailStage()
    {
        SoundManager_1.Instance.PlaySfx(SfxType.Fail);


        isFailed = true;

        Debug.Log("스테이지 실패");

        if (failPanel != null)
        {
            failPanel.SetActive(true);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetFail();
        }
    }

    private void UpdateStageUI()
    {
        if (targetText != null)
        {
            targetText.text = $"목표: {targetStoneLevel}돌 만들기";
        }

        if (stageNameText != null && currentStageData != null)
        {
            stageNameText.text = currentStageData.stageName;
        }
    }

    public void ForceFail()
    {
        if (isCleared || isFailed)
        {
            return;
        }

        FailStage();
    }
}