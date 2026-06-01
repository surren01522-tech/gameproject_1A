using UnityEngine;

public enum TurnState
{
    PlayerReady,
    PlayerMoving,
    TurnEnd
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("State")]
    [SerializeField] private TurnState currentTurnState = TurnState.PlayerReady;

    public TurnState CurrentTurnState => currentTurnState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool CanPlayerShoot()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying())
        {
            return false;
        }

        return currentTurnState == TurnState.PlayerReady;
    }

    public void StartPlayerMove()
    {
        currentTurnState = TurnState.PlayerMoving;
        Debug.Log("Player move started.");
    }

    public void EndPlayerMove()
    {
        currentTurnState = TurnState.TurnEnd;
        Debug.Log("Player move ended.");

        if (StageManager.Instance != null)
        {
            StageManager.Instance.AddTurnCount();
            StageManager.Instance.RequestFailCheck();
        }

        StartNextTurn();
    }

    private void StartNextTurn()
    {
        currentTurnState = TurnState.PlayerReady;
        Debug.Log("Next turn started.");
    }
}
