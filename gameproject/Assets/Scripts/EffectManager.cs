using UnityEngine;

public enum EffectType
{
    Shoot,
    HitWall,
    HitEnemy,
    StoneBreak,
    EnemyDead,
    Merge,
    Clear,
    Fail,
    TurnStart
}

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [Header("Effect Prefabs")]
    [SerializeField] private GameObject shootEffectPrefab;
    [SerializeField] private GameObject hitWallEffectPrefab;
    [SerializeField] private GameObject hitEnemyEffectPrefab;
    [SerializeField] private GameObject stoneBreakEffectPrefab;
    [SerializeField] private GameObject enemyDeadEffectPrefab;
    [SerializeField] private GameObject mergeEffectPrefab;
    [SerializeField] private GameObject clearEffectPrefab;
    [SerializeField] private GameObject failEffectPrefab;
    [SerializeField] private GameObject turnStartEffectPrefab;

    [Header("Parent")]
    [SerializeField] private Transform effectParent;

    [Header("Destroy")]
    [SerializeField] private float defaultDestroyTime = 2f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject PlayEffect(EffectType type, Vector3 position)
    {
        GameObject prefab = GetEffectPrefab(type);

        if (prefab == null)
        {
            return null;
        }

        GameObject effectObject = Instantiate(
            prefab,
            position,
            Quaternion.identity,
            effectParent
        );

        Destroy(effectObject, defaultDestroyTime);

        return effectObject;
    }

    public GameObject PlayEffect(EffectType type, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = GetEffectPrefab(type);

        if (prefab == null)
        {
            return null;
        }

        GameObject effectObject = Instantiate(
            prefab,
            position,
            rotation,
            effectParent
        );

        Destroy(effectObject, defaultDestroyTime);

        return effectObject;
    }

    private GameObject GetEffectPrefab(EffectType type)
    {
        switch (type)
        {
            case EffectType.Shoot:
                return shootEffectPrefab;

            case EffectType.HitWall:
                return hitWallEffectPrefab;

            case EffectType.HitEnemy:
                return hitEnemyEffectPrefab;

            case EffectType.StoneBreak:
                return stoneBreakEffectPrefab;

            case EffectType.EnemyDead:
                return enemyDeadEffectPrefab;

            case EffectType.Merge:
                return mergeEffectPrefab;

            case EffectType.Clear:
                return clearEffectPrefab;

            case EffectType.Fail:
                return failEffectPrefab;

            case EffectType.TurnStart:
                return turnStartEffectPrefab;
        }

        return null;
    }
}