using UnityEngine;

public enum GameState
{
    Ready,
    Playing,
    Clear,
    Fail
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private bool startOnAwake = true;

    public GameState CurrentState { get; private set; } = GameState.Playing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentState = startOnAwake ? GameState.Playing : GameState.Ready;
    }

    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        Debug.Log("Game State: Playing");
    }

    public void RestartGame()
    {
        CurrentState = GameState.Playing;
        Debug.Log("Game State: Restart");
    }

    public void SetClear()
    {
        if (CurrentState == GameState.Clear || CurrentState == GameState.Fail)
        {
            return;
        }

        CurrentState = GameState.Clear;
        Debug.Log("Game State: Clear");
    }

    public void SetFail()
    {
        if (CurrentState == GameState.Clear || CurrentState == GameState.Fail)
        {
            return;
        }

        CurrentState = GameState.Fail;
        Debug.Log("Game State: Fail");
    }
}
