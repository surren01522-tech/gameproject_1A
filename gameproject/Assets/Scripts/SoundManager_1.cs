using UnityEngine;

public class SoundManager_1 : MonoBehaviour
{
    public static SoundManager_1 Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource bgmAudioSource;

    [Header("SFX Pool")]
    [SerializeField] private int sfxSourceCount = 10;
    private AudioSource[] sfxAudioSources;
    private int currentSfxIndex = 0;

    [Header("BGM Clip (Long Audio)")]
    [SerializeField] private AudioClip mainBgmClip;

    [Header("UI Clips")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip panelOpenClip;
    [SerializeField] private AudioClip panelCloseClip;

    [Header("Player Clips")]
    [SerializeField] private AudioClip dragStartClip;
    [SerializeField] private AudioClip shootClip;

    [Header("Hit Clips")]
    [SerializeField] private AudioClip hitWallClip;
    [SerializeField] private AudioClip hitEnemyClip;

    [Header("Destroy Clips")]
    [SerializeField] private AudioClip stoneBreakClip;
    [SerializeField] private AudioClip enemyDeadClip;

    [Header("Game Clips")]
    [SerializeField] private AudioClip mergeClip;
    [SerializeField] private AudioClip turnStartClip;
    [SerializeField] private AudioClip clearClip;
    [SerializeField] private AudioClip failClip;

    [Header("Master Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 0.3f;

    [Header("Individual SFX Volume - UI")]
    [Range(0f, 1f)]
    [SerializeField] private float buttonClickVolume = 0.6f;

    [Range(0f, 1f)]
    [SerializeField] private float panelOpenVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float panelCloseVolume = 0.7f;

    [Header("Individual SFX Volume - Player")]
    [Range(0f, 1f)]
    [SerializeField] private float dragStartVolume = 0.3f;

    [Range(0f, 1f)]
    [SerializeField] private float shootVolume = 0.8f;

    [Header("Individual SFX Volume - Hit")]
    [Range(0f, 1f)]
    [SerializeField] private float hitWallVolume = 0.5f;

    [Range(0f, 1f)]
    [SerializeField] private float hitEnemyVolume = 0.6f;

    [Header("Individual SFX Volume - Destroy")]
    [Range(0f, 1f)]
    [SerializeField] private float stoneBreakVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float enemyDeadVolume = 0.8f;

    [Header("Individual SFX Volume - Game")]
    [Range(0f, 1f)]
    [SerializeField] private float mergeVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float turnStartVolume = 0.5f;

    [Range(0f, 1f)]
    [SerializeField] private float clearVolume = 0.9f;

    [Range(0f, 1f)]
    [SerializeField] private float failVolume = 0.9f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("[SoundManager] 중복 SoundManager가 있어서 삭제합니다.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateSfxAudioSourcePool();
        SetupBgmAudioSource();

        PlayMainBgm();
    }

    private void CreateSfxAudioSourcePool()
    {
        if (sfxSourceCount <= 0)
        {
            sfxSourceCount = 10;
        }

        sfxAudioSources = new AudioSource[sfxSourceCount];

        for (int i = 0; i < sfxSourceCount; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.loop = false;
            source.volume = 1f;
            source.spatialBlend = 0f;

            sfxAudioSources[i] = source;
        }

        Debug.Log($"[SoundManager] SFX AudioSource Pool 생성 완료: {sfxSourceCount}개");
    }

    private void SetupBgmAudioSource()
    {
        if (bgmAudioSource == null)
        {
            bgmAudioSource = gameObject.AddComponent<AudioSource>();
        }

        bgmAudioSource.playOnAwake = false;
        bgmAudioSource.loop = true;
        bgmAudioSource.volume = bgmVolume;
        bgmAudioSource.spatialBlend = 0f;
    }

    public static void Play(SfxType type)
    {
        if (Instance == null)
        {
            Debug.LogWarning("[SoundManager] Instance가 없습니다. Scene에 SoundManager_1이 있는지 확인하세요.");
            return;
        }

        Instance.PlaySfx(type);
    }

    public void PlaySfx(SfxType type)
    {
        AudioClip clip = GetClip(type);

        if (clip == null)
        {
            Debug.LogWarning($"[SoundManager] {type} 효과음 클립이 연결되지 않았습니다.");
            return;
        }

        if (sfxAudioSources == null || sfxAudioSources.Length == 0)
        {
            Debug.LogWarning("[SoundManager] SFX AudioSource Pool이 없습니다.");
            return;
        }

        AudioSource source = GetAvailableSfxSource();

        if (source == null)
        {
            Debug.LogWarning("[SoundManager] 사용할 수 있는 SFX AudioSource가 없습니다.");
            return;
        }

        float individualVolume = GetIndividualVolume(type);
        float finalVolume = sfxVolume * individualVolume;

        source.PlayOneShot(clip, finalVolume);

        Debug.Log($"[SoundManager] SFX 재생: {type} / Clip: {clip.name} / Volume: {finalVolume}");
    }

    public void PlayBgm(AudioClip bgmClip)
    {
        if (bgmAudioSource == null)
        {
            Debug.LogWarning("[SoundManager] BGM AudioSource가 없습니다.");
            return;
        }

        if (bgmClip == null)
        {
            Debug.LogWarning("[SoundManager] 재생할 BGM Clip이 없습니다.");
            return;
        }

        if (bgmAudioSource.isPlaying && bgmAudioSource.clip == bgmClip)
        {
            return;
        }

        bgmAudioSource.clip = bgmClip;
        bgmAudioSource.volume = bgmVolume;
        bgmAudioSource.Play();

        Debug.Log($"[SoundManager] BGM 재생: {bgmClip.name}");
    }

    public void PlayMainBgm()
    {
        if (mainBgmClip == null)
        {
            Debug.LogWarning("[SoundManager] Main BGM Clip이 연결되지 않았습니다.");
            return;
        }

        PlayBgm(mainBgmClip);
    }

    public void StopBgm()
    {
        if (bgmAudioSource == null)
        {
            return;
        }

        bgmAudioSource.Stop();
        Debug.Log("[SoundManager] BGM 정지");
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        Debug.Log($"[SoundManager] 전체 SFX 볼륨 변경: {sfxVolume}");
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = bgmVolume;
        }

        Debug.Log($"[SoundManager] BGM 볼륨 변경: {bgmVolume}");
    }

    private AudioSource GetAvailableSfxSource()
    {
        for (int i = 0; i < sfxAudioSources.Length; i++)
        {
            if (sfxAudioSources[i] != null && !sfxAudioSources[i].isPlaying)
            {
                return sfxAudioSources[i];
            }
        }

        AudioSource source = sfxAudioSources[currentSfxIndex];

        currentSfxIndex++;

        if (currentSfxIndex >= sfxAudioSources.Length)
        {
            currentSfxIndex = 0;
        }

        return source;
    }

    private float GetIndividualVolume(SfxType type)
    {
        switch (type)
        {
            case SfxType.ButtonClick:
                return buttonClickVolume;

            case SfxType.PanelOpen:
                return panelOpenVolume;

            case SfxType.PanelClose:
                return panelCloseVolume;

            case SfxType.DragStart:
                return dragStartVolume;

            case SfxType.Shoot:
                return shootVolume;

            case SfxType.HitWall:
                return hitWallVolume;

            case SfxType.HitEnemy:
                return hitEnemyVolume;

            case SfxType.StoneBreak:
                return stoneBreakVolume;

            case SfxType.EnemyDead:
                return enemyDeadVolume;

            case SfxType.Merge:
                return mergeVolume;

            case SfxType.TurnStart:
                return turnStartVolume;

            case SfxType.Clear:
                return clearVolume;

            case SfxType.Fail:
                return failVolume;
        }

        return 1f;
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