using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Slingshot/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("Stage Info")]
    public string stageName = "Stage 1";

    [Header("Goal")]
    public int targetStoneLevel = 3;

    [Header("Initial Stones")]
    public List<StageStoneData> initialStones = new List<StageStoneData>();

    [Header("Initial Enemies")]
    public List<StageEnemyData> initialEnemies = new List<StageEnemyData>();
}

[System.Serializable]
public class StageStoneData
{
    public int level = 1;
    public int hp = 3;
    public Vector2 position;
}

[System.Serializable]
public class StageEnemyData
{
    public GameObject enemyPrefab;
    public int hp = 1;
    public Vector2 position;
}