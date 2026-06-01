using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Slingshot/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("Stage Info")]
    public int stageNumber = 1;
    public string stageName = "Stage 1";

    [Header("Goal")]
    public int targetStoneLevel = 3;
    public int maxTurnCount = 0;

    [Header("Defaults")]
    public int defaultStoneHp = 3;
    public int defaultEnemyHp = 1;

    [Header("Initial Stones")]
    public List<StageStoneData> initialStones = new List<StageStoneData>();

    [Header("Initial Enemies")]
    public List<StageEnemyData> initialEnemies = new List<StageEnemyData>();

    public string TargetDescription => $"Create a level {targetStoneLevel} stone";

    public bool ValidateStage(out string errorMessage)
    {
        if (targetStoneLevel <= 0)
        {
            errorMessage = "Target stone level must be 1 or higher.";
            return false;
        }

        if (initialStones == null || initialStones.Count <= 0)
        {
            errorMessage = "At least one initial stone is required.";
            return false;
        }

        errorMessage = "";
        return true;
    }

    private void OnValidate()
    {
        stageNumber = Mathf.Max(1, stageNumber);
        targetStoneLevel = Mathf.Max(1, targetStoneLevel);
        maxTurnCount = Mathf.Max(0, maxTurnCount);
        defaultStoneHp = Mathf.Max(1, defaultStoneHp);
        defaultEnemyHp = Mathf.Max(1, defaultEnemyHp);

        if (initialStones == null)
        {
            initialStones = new List<StageStoneData>();
        }

        if (initialEnemies == null)
        {
            initialEnemies = new List<StageEnemyData>();
        }

        for (int i = 0; i < initialStones.Count; i++)
        {
            initialStones[i].Normalize(defaultStoneHp);
        }

        for (int i = 0; i < initialEnemies.Count; i++)
        {
            initialEnemies[i].Normalize(defaultEnemyHp);
        }
    }
}

[System.Serializable]
public class StageStoneData
{
    public int level = 1;
    public int hp = 3;
    public Vector2 position;

    public void Normalize(int fallbackHp)
    {
        level = Mathf.Max(1, level);
        hp = hp <= 0 ? Mathf.Max(1, fallbackHp) : hp;
    }
}

[System.Serializable]
public class StageEnemyData
{
    public GameObject enemyPrefab;
    public int hp = 1;
    public Vector2 position;

    public void Normalize(int fallbackHp)
    {
        hp = hp <= 0 ? Mathf.Max(1, fallbackHp) : hp;
    }
}
