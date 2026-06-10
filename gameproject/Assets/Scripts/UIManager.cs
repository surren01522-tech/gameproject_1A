using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public Toggle bgmToggle;
    public Toggle fxToggle;

    public Button openButton;
    public Button closeButton;

    public Slider bgmSlider;
    public Slider fxSlider;

    public GameObject panel;

    private void Awake()
    {
        bgmToggle.onValueChanged.AddListener(OnBGMToggleChange);
        fxToggle.onValueChanged.AddListener(OnFXToggleChange);

        openButton.onClick.AddListener(OpenOptionPanel);
        closeButton.onClick.AddListener(CloseOptionPanel);

        bgmSlider.onValueChanged.AddListener(OnBGMSliderChange);
        fxSlider.onValueChanged.AddListener(OnFxSliderChange);
    }

    private void OnBGMToggleChange(bool isOn)
    {
        SoundManager_1.Instance.SetBgmOn(isOn);
        SoundManager_1.Instance.PlaySfx(SfxType.ButtonClick);
    }

    private void OnFXToggleChange(bool isOn)
    {
        SoundManager_1.Instance.SetSfxOn(isOn);
        SoundManager_1.Instance.PlaySfx(SfxType.ButtonClick);
    }

    private void OnBGMSliderChange(float volume)
    {
        SoundManager_1.Instance.SetBgmVolume(volume);
    }

    private void OnFxSliderChange(float volume)
    {
        SoundManager_1.Instance.SetSfxVolume(volume);
    }

    private void OpenOptionPanel()
    {
        panel.SetActive(true);
        SoundManager_1.Instance.PlaySfx(SfxType.ButtonClick);
    }

    private void CloseOptionPanel()
    {
        panel.SetActive(false);
        SoundManager_1.Instance.PlaySfx(SfxType.ButtonClick);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("StageSelectScene");
    }
}
