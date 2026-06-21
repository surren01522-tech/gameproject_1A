using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;

    [Header("BGM UI")]
    [SerializeField] private Toggle bgmToggle;
    [SerializeField] private Slider bgmSlider;

    [Header("SFX UI")]
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Slider sfxSlider;

    [Header("Scene")]
    [SerializeField] private string titleSceneName = "TitleScene";

    private bool isPaused = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        InitUI();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void InitUI()
    {
        if (bgmToggle != null)
        {
            bgmToggle.onValueChanged.AddListener(OnBgmToggleChanged);
        }

        if (sfxToggle != null)
        {
            sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Time.timeScale = 0f;

        SoundManager_1.Play(SfxType.PanelOpen);
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;

        SoundManager_1.Play(SfxType.PanelClose);
    }

    public void GoToTitle()
    {
        Time.timeScale = 1f;

        SoundManager_1.Play(SfxType.ButtonClick);

        SceneManager.LoadScene(titleSceneName);
    }

    private void OnBgmToggleChanged(bool isOn)
    {
        if (SoundManager_1.Instance == null) return;

        SoundManager_1.Instance.SetBgmOn(isOn);
        SoundManager_1.Play(SfxType.ButtonClick);
    }

    private void OnSfxToggleChanged(bool isOn)
    {
        if (SoundManager_1.Instance == null) return;

        SoundManager_1.Instance.SetSfxOn(isOn);
        SoundManager_1.Play(SfxType.ButtonClick);
    }

    private void OnBgmVolumeChanged(float value)
    {
        if (SoundManager_1.Instance == null) return;

        SoundManager_1.Instance.SetBgmVolume(value);
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (SoundManager_1.Instance == null) return;

        SoundManager_1.Instance.SetSfxVolume(value);
    }
}