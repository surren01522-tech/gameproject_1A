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
    [SerializeField] private Transform wallParent;

    [Header("UI")]
    [SerializeField] private Text targetText;
    [SerializeField] private Text stageNameText;
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private GameObject failPanel;

    [Header("Physics Feel")]
    [SerializeField] private float stoneLinearDamping = 1.25f;
    [SerializeField] private float stoneAngularDamping = 1.0f;
    [SerializeField] private float enemyLinearDamping = 1.6f;
    [SerializeField] private float enemyAngularDamping = 1.2f;

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

        SpawnStageWalls();
        SpawnStageStones();
        SpawnStageEnemies();
    }

    private void SpawnStageWalls()
    {
        for (int i = 0; i < currentStageData.stageWalls.Count; i++)
        {
            StageWallData wallData = currentStageData.stageWalls[i];
            GameObject wallObject = new GameObject($"StageWall_{i + 1}");
            wallObject.tag = "Wall";
            wallObject.transform.SetParent(wallParent != null ? wallParent : transform, false);
            wallObject.transform.position = wallData.position;
            wallObject.transform.localScale = new Vector3(wallData.size.x, wallData.size.y, 1f);

            BoxCollider2D collider = wallObject.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
        }
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
            ConfigureTopDownBody(stoneObject.GetComponent<Rigidbody2D>(), stoneLinearDamping, stoneAngularDamping);
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

            ConfigureTopDownBody(enemyObject.GetComponent<Rigidbody2D>(), enemyLinearDamping, enemyAngularDamping);
        }
    }

    private void ConfigureTopDownBody(Rigidbody2D rb, float linearDamping, float angularDamping)
    {
        if (rb == null)
        {
            return;
        }

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.linearDamping = linearDamping;
        rb.angularDamping = angularDamping;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
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

}
