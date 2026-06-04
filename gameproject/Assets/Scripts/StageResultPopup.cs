using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageResultPopup : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string stageSelectSceneName = "StageSelectScene";

    [Header("Button")]
    [SerializeField] private Button goToStageSelectButton;

    private void Awake()
    {
        if (goToStageSelectButton != null)
        {
            goToStageSelectButton.onClick.RemoveListener(GoToStageSelectScene);
            goToStageSelectButton.onClick.AddListener(GoToStageSelectScene);
        }
    }

    public void GoToStageSelectScene()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(SfxType.ButtonClick);
        }

        SceneManager.LoadScene(stageSelectSceneName);
    }
}