using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour //타이틀에 넣어두고 씬에 따라 BGM 변경하는 코드.
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer mainMixer; //MainMixer 연결, 나머지 그룹도 배치하여 슬라이더 바 적용
    public AudioMixerGroup bgmGroup;
    public AudioMixerGroup sfxGroup;

    private const string BGM_PARAM = "BGM";
    private const string SFX_PARAM = "SFX";

    private const string MASTER_PARAM = "Master"; // 마스터 볼륨 파라미터 추가

    [Header("Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Background Music")]
    public AudioClip titleBGM;
     public AudioClip prologueTheme; // 컷신 + 프롤로그용
    public AudioClip stageBGM;
    public AudioClip bossBGM;   // 보스전 음악

    public AudioClip EndingBGM;

    // ★ 보스전 BGM 복귀를 위한 변수
    private AudioClip savedBGM; 

    [Header("Master Volume Settings (0 ~ 1)")]
    // 에러 해결: 변수명을 masterSFXScale로 통일합니다.
    [Range(0f, 1f)] public float masterSFXScale = 0.5f;

    private void Awake()
    {
        // 씬이 바뀌어도 파괴되지 않게 설정 (타이틀에서 인게임 전환하는 경우 등)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // 이벤트 등록
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
 
        
        // 게임을 켜자마자 저장된 설정(볼륨) 불러오기
        LoadAllSettings(); // 볼륨 및 해상도 설정 통합 로드

        // 맨 처음 시작할 때 현재 씬 음악 틀기 (Start 시점)
        PlayMusicForScene(SceneManager.GetActiveScene().name);

        
    }

    private void OnDestroy()
    {
        // 이벤트 등록 해제 
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드 시마다 볼륨을 다시 확인하여 믹서 튐 방지
        LoadVolumeSettings();

        // 씬 이름에 따라 BGM 자동 재생 
        PlayMusicForScene(scene.name);
    }

    // 씬 이름에 따라 음악 결정
    private void PlayMusicForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Title":
                PlayBGM(titleBGM);
                break;

            // 컷신과 프롤로그는 같은 음악을 공유하여 게임 몰입 유지
            case "Cutscene_Intro":      // 컷신 
            case "Prologue":      // 프롤로그 씬 
                PlayBGM(prologueTheme); 
                break;
  
            case "Prologue2":
            case "Stage1":
            case "Stage1_Boss":
            case "Stage2":
            case "Stage2_Boss":
                PlayBGM(stageBGM); 
                break;
            
            case "Scene_Ending": //엔딩 씬
                PlayBGM(EndingBGM);
                break;

            default:
                break;
        }
    }



    //BGM 관리
    public void PlayBGM(AudioClip clip)
    {
       if (clip == null || bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    //보스전 진입: 현재 노래 기억 > 보스 노래 재생
    public void PlayBossBGM(AudioClip bossClip)
    {
        savedBGM = bgmSource.clip; // 원래 듣던 노래 저장
        PlayBGM(bossClip);
    }

    // 보스전 종료: 원래 음악 재생
    public void StopBossBGM()
    {
        if (savedBGM != null)
        {
            PlayBGM(savedBGM);
            savedBGM = null;
        }
    }


    // 효과음
    public void PlaySFX(AudioClip clip)
    {
     if (clip == null) return;
        // 저장된 SFX 설정값과 마스터 스케일을 곱해 2중 잠금
        float savedVol = PlayerPrefs.GetFloat(SFX_PARAM, 0.5f);
        sfxSource.PlayOneShot(clip, savedVol * masterSFXScale);
    }
    
    // SettingsUI와 VolumeSlider에서 공통으로 사용할 함수
    public void SetVolume(string parameterName, float volume) 
    {
        float db = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20f;
        mainMixer.SetFloat(parameterName, db);

        PlayerPrefs.SetFloat(parameterName, volume);
        PlayerPrefs.Save();
    }

    // 기존 VolumeSlider.cs 함수 가져옴
    public void SetBGMVolume(float volume) => SetVolume(BGM_PARAM, volume);
public void SetSFXVolume(float volume) => SetVolume(SFX_PARAM, volume);

public void LoadAllSettings() => LoadVolumeSettings();

    // 초기 실행 시 저장된 볼륨 적용
   public void LoadVolumeSettings()
    {
       
  string[] paramsToLoad = { "Master", "BGM", "SFX" };
        foreach (var param in paramsToLoad)
        {
            float vol = PlayerPrefs.GetFloat(param, 0.5f);
            float db = (vol <= 0.0001f) ? -80f : Mathf.Log10(vol) * 20f;
            mainMixer.SetFloat(param, db);
        }
        Debug.Log("[Mixer] 모든 볼륨 설정 복구 완료");
    }
}