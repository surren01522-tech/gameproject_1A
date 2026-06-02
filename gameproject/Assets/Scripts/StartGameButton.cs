using UnityEngine;

public class StartGameButton : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;

    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }

        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }
    }
}
