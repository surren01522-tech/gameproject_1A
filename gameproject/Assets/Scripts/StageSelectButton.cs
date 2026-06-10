using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SelectedStageData
{
    public static StageData CurrentStageData;
}

[RequireComponent(typeof(Button))]
public class StageSelectButton : MonoBehaviour
{
    [Header("Stage")]
    [SerializeField] private StageData stageData;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("UI")]
    [SerializeField] private Text stageNameText;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        button.onClick.RemoveListener(SelectStage);
        button.onClick.AddListener(SelectStage);
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (stageNameText != null && stageData != null)
        {
            stageNameText.text = stageData.stageName;
        }
    }

    private void SelectStage()
    {
        if (stageData == null)
        {
            Debug.LogError($"{gameObject.name}에 StageData가 연결되지 않았습니다.");
            return;
        }

        SoundManager_1.Instance.PlaySfx(SfxType.ButtonClick);

        SelectedStageData.CurrentStageData = stageData;

        SceneManager.LoadScene(gameSceneName);
    }
}