using UnityEngine;

public enum GameState
{
    Playing,
    Clear,
    Fail
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Playing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentState = GameState.Playing;
    }

    public bool IsPlaying()
    {
        return CurrentState == GameState.Playing;
    }

    public void SetClear()
    {
        CurrentState = GameState.Clear;
        Debug.Log("Game State: Clear");
    }

    public void SetFail()
    {
        CurrentState = GameState.Fail;
        Debug.Log("Game State: Fail");
    }
}