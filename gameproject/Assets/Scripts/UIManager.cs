using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public Toggle bgmToggle;

    public Button openButton;
    public Button closeButton;

    public Slider bgmSlider;

    public GameObject panel;

    private void Awake()
    {
        bgmToggle.onValueChanged.AddListener(OnBGMToggleChange);

        openButton.onClick.AddListener(OpenOptionPanel);
        closeButton.onClick.AddListener(CloseOptionPanel);

        bgmSlider.onValueChanged.AddListener(OnBGMSliderChange);
    }

    private void OnBGMToggleChange(bool isOn)
    {
        SoundManager.Instance.StopBgm();
    }

    private void OnBGMSliderChange(float volume)
    {
        SoundManager.Instance.SetBgmVolume(volume);
    }

    private void OpenOptionPanel()
    {
        panel.SetActive(true);
    }

    private void CloseOptionPanel()
    {
        panel.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("StageSelectScene");
    }
}
