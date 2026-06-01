using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    Clear,
    Fail
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("State")]
    [SerializeField] private GameState currentState = GameState.Playing;
    [SerializeField] private string currentStageName = "";

    public GameState CurrentState => currentState;
    public string CurrentStageName => currentStageName;

    public event System.Action<GameState> StateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartGame(StageData stageData)
    {
        currentStageName = stageData != null ? stageData.stageName : "";
        ChangeState(GameState.Playing);
    }

    public bool IsPlaying()
    {
        return currentState == GameState.Playing;
    }

    public void SetClear()
    {
        ChangeState(GameState.Clear);
    }

    public void SetFail()
    {
        ChangeState(GameState.Fail);
    }

    public void RestartCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }

    private void ChangeState(GameState nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        currentState = nextState;
        Debug.Log($"Game State: {currentState}");
        StateChanged?.Invoke(currentState);
    }
}
