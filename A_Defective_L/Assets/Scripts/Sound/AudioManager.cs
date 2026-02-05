using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer mainMixer; // ★ 아까 만든 MainMixer 연결
    public AudioMixerGroup bgmGroup;
    public AudioMixerGroup sfxGroup;

    [Header("Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Background Music")]
    public AudioClip titleBGM;
     public AudioClip prologueTheme; // 컷신 + 프롤로그용
    public AudioClip stageBGM;
    public AudioClip bossBGM;   // 보스전 음악

    // ★ 보스전 BGM 복귀를 위한 변수
    private AudioClip savedBGM; 

    private void Awake()
    {
        // 씬이 바뀌어도 파괴되지 않게 설정 (타이틀 <-> 인게임)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 씬이 로드될 때마다 적절한 BGM 재생 (옵션)
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // ★ 게임 켜자마자 저장된 설정(볼륨) 불러오기
        LoadVolumeSettings();

        // 4. 맨 처음 시작할 때 현재 씬 음악 틀기 (Start 시점)
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        // 이벤트 등록 해제 (습관적으로 해두는 게 좋음)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);

        // 씬 이름에 따라 BGM 자동 재생 (필요하면 사용)
        if (scene.name == "Title") PlayBGM(titleBGM);
        else if (scene.name == "Main") PlayBGM(stageBGM);
    }

    // 씬 이름에 따라 음악 결정
    private void PlayMusicForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Title":
                PlayBGM(titleBGM);
                break;

            // ★ [추가] 컷신과 프롤로그는 같은 음악을 공유 (끊김 없이 재생됨)
            case "Cutscene_Intro":      // 컷신 씬 이름
            case "Prologue":      // 프롤로그 씬 이름
            case "Prologue2":
                PlayBGM(prologueTheme); 
                break;

            case "Main":      
            case "Stage1":
            case "Stage1_Boss":
                PlayBGM(stageBGM); // 이제 빨간줄 안 뜸
                break;
            
            default:
                break;
        }
    }


    // ====================================================
    // 🎵 BGM 관리 (보스전 기능 포함)
    // ====================================================
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip) return; // 이미 재생 중이면 패스

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // ★ 보스전 진입: 현재 노래 기억해두고 보스 노래 틀기
    public void PlayBossBGM(AudioClip bossClip)
    {
        savedBGM = bgmSource.clip; // 원래 듣던 노래 저장
        PlayBGM(bossClip);
    }

    // ★ 보스전 종료: 원래 노래로 복귀
    public void StopBossBGM()
    {
        if (savedBGM != null)
        {
            PlayBGM(savedBGM);
            savedBGM = null;
        }
    }

    // ====================================================
    // 🔊 SFX (효과음) 관리
    // ====================================================
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        // 효과음은 중첩될 수 있으므로 PlayOneShot 사용
        sfxSource.PlayOneShot(clip); 
    }
    
    // 랜덤 피치 등의 옵션이 필요하면 오버로딩해서 구현 가능

    // ====================================================
    // 🎚️ 볼륨 조절 & 저장 (믹서 연동)
    // ====================================================
    public void SetBGMVolume(float volume) // 슬라이더 값 (0.0001 ~ 1.0)
    {
        // 로그 스케일 변환 (슬라이더는 선형, 소리는 로그)
        float db = Mathf.Log10(volume) * 20; 
        if (volume <= 0.0001f) db = -80f; // 음소거 처리

        mainMixer.SetFloat("BGM", db);
        
        // ★ 변경 즉시 저장 (PlayerPrefs)
        PlayerPrefs.SetFloat("BGM_Volume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        float db = Mathf.Log10(volume) * 20;
        if (volume <= 0.0001f) db = -80f;

        mainMixer.SetFloat("SFX", db);

        PlayerPrefs.SetFloat("SFX_Volume", volume);
        PlayerPrefs.Save();
    }

    // 초기 실행 시 저장된 볼륨 적용
    public void LoadVolumeSettings()
    {
        float bgmVol = PlayerPrefs.GetFloat("BGM_Volume", 1.0f); // 기본값 1
        float sfxVol = PlayerPrefs.GetFloat("SFX_Volume", 1.0f);

        // 믹서에 적용 (Set 함수 재사용 시 저장 로직이 중복되므로 직접 설정)
        float bgmDb = Mathf.Log10(bgmVol) * 20;
        if (bgmVol <= 0.0001f) bgmDb = -80f;
        mainMixer.SetFloat("BGM", bgmDb);

        float sfxDb = Mathf.Log10(sfxVol) * 20;
        if (sfxVol <= 0.0001f) sfxDb = -80f;
        mainMixer.SetFloat("SFX", sfxDb);
    }
}