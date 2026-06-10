using UnityEngine;

public enum TurnState
{
    PlayerReady,     // 플레이어가 조준할 수 있는 상태
    PlayerMoving,    // 발사 후 움직이는 상태
    TurnEnd          // 멈춘 뒤 턴 종료 처리 상태
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

        Debug.Log("플레이어 이동 시작");
    }

    public void EndPlayerMove()
    {
        currentTurnState = TurnState.TurnEnd;

        Debug.Log("플레이어 이동 종료");

        // 여기에서 합성/실패 체크를 요청할 수 있음
        if (StageManager.Instance != null)
        {
            StageManager.Instance.RequestFailCheck();
        }

        StartNextTurn();
    }

    private void StartNextTurn()
    {
        SoundManager_1.Instance.PlaySfx(SfxType.TurnStart);
        currentTurnState = TurnState.PlayerReady;

        Debug.Log("다음 턴 시작! 다시 발사 가능");
    }
}