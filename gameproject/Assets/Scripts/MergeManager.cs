using UnityEngine;

public class MergeManager : MonoBehaviour
{
    public static MergeManager Instance { get; private set; }

    [Header("Stone Prefab")]
    [SerializeField] private GameObject stonePrefab;

    [Header("Merge Setting")]
    [SerializeField] private int mergedStoneDefaultHp = 3;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TryMerge(Stone stoneA, Stone stoneB)
    {
        if (stoneA == null || stoneB == null)
        {
            return;
        }

        if (stoneA == stoneB)
        {
            return;
        }

        if (stoneA.isMerging || stoneB.isMerging)
        {
            return;
        }

        if (stoneA.level != stoneB.level)
        {
            return;
        }

        stoneA.isMerging = true;
        stoneB.isMerging = true;

        Vector3 mergePosition = GetMergePosition(stoneA, stoneB);
        int newLevel = stoneA.level + 1;

        Destroy(stoneA.gameObject);
        Destroy(stoneB.gameObject);

        CreateMergedStone(newLevel, mergePosition);
    }

    private Vector3 GetMergePosition(Stone stoneA, Stone stoneB)
    {
        return (stoneA.transform.position + stoneB.transform.position) * 0.5f;
    }

    private void CreateMergedStone(int level, Vector3 position)
    {
        if (stonePrefab == null)
        {
            Debug.LogError("MergeManager에 stonePrefab이 연결되지 않았습니다.");
            return;
        }

        GameObject newStoneObject = Instantiate(stonePrefab, position, Quaternion.identity);

        Stone newStone = newStoneObject.GetComponent<Stone>();

        if (newStone == null)
        {
            Debug.LogError("생성된 stonePrefab에 Stone.cs가 없습니다.");
            return;
        }

        newStone.Initialize(level, mergedStoneDefaultHp);

        newStoneObject.name = $"Stone_Level_{level}";

        Rigidbody2D rb = newStoneObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        StageManager.Instance.CheckClear(level);
    }
}