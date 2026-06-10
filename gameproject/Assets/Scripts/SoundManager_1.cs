using UnityEngine;

public class SoundManager_1 : MonoBehaviour
{
    public static SoundManager_1 Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource bgmAudioSource;

    [Header("BGM Clip (Long Audio)")]
    [SerializeField] private AudioClip mainBgmClip;

    [Header("UI")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip panelOpenClip;
    [SerializeField] private AudioClip panelCloseClip;

    [Header("Player")]
    [SerializeField] private AudioClip dragStartClip;
    [SerializeField] private AudioClip shootClip;

    [Header("Hit")]
    [SerializeField] private AudioClip hitWallClip;
    [SerializeField] private AudioClip hitEnemyClip;

    [Header("Destroy")]
    [SerializeField] private AudioClip stoneBreakClip;
    [SerializeField] private AudioClip enemyDeadClip;

    [Header("Game")]
    [SerializeField] private AudioClip mergeClip;
    [SerializeField] private AudioClip turnStartClip;
    [SerializeField] private AudioClip clearClip;
    [SerializeField] private AudioClip failClip;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }

        sfxAudioSource.playOnAwake = false;
        sfxAudioSource.volume = sfxVolume;

        if (bgmAudioSource == null)
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
        }
        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.loop = true;
        bgmAudioSource.volume = bgmVolume;

        PlayMainBgm();
    }

    public void PlaySfx(SfxType type)
    {
        AudioClip clip = GetClip(type);

        if (clip == null)
        {
            return;
        }

        if (sfxAudioSource == null)
        {
            return;
        }

        sfxAudioSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayBgm(AudioClip bgmClip)
    {
        if (bgmAudioSource == null || bgmClip == null) return;
        if (bgmAudioSource.isPlaying && bgmAudioSource.clip == bgmClip) return;

        bgmAudioSource.clip = bgmClip;
        bgmAudioSource.Play();
    }

    public void PlayMainBgm()
    {
        if (bgmAudioSource == null || mainBgmClip == null) return;
        if (bgmAudioSource.isPlaying && bgmAudioSource.clip == mainBgmClip) return;

        bgmAudioSource.clip = mainBgmClip;
        bgmAudioSource.Play();
    }

    public void StopBgm()
    {
        if (bgmAudioSource == null)
        {
            return;
        }

        bgmAudioSource.Stop();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = sfxVolume;
        }
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = bgmVolume;
        }
    }

    private AudioClip GetClip(SfxType type)
    {
        switch (type)
        {
            case SfxType.ButtonClick:
                return buttonClickClip;

            case SfxType.PanelOpen:
                return panelOpenClip;

            case SfxType.PanelClose:
                return panelCloseClip;

            case SfxType.DragStart:
                return dragStartClip;

            case SfxType.Shoot:
                return shootClip;

            case SfxType.HitWall:
                return hitWallClip;

            case SfxType.HitEnemy:
                return hitEnemyClip;

            case SfxType.StoneBreak:
                return stoneBreakClip;

            case SfxType.EnemyDead:
                return enemyDeadClip;

            case SfxType.Merge:
                return mergeClip;

            case SfxType.TurnStart:
                return turnStartClip;

            case SfxType.Clear:
                return clearClip;

            case SfxType.Fail:
                return failClip;
        }

        return null;
    }
}