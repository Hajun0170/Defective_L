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

    

    private void Awake()
    {
        // 씬이 바뀌어도 파괴되지 않게 설정 (타이틀에서 인게임 전환하는 경우 등)
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
        // 씬이 로드될 때마다 BGM 재생
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // 게임을 켜자마자 저장된 설정(볼륨) 불러오기
        LoadVolumeSettings();

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
        PlayMusicForScene(scene.name);

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
        if (clip == null) return;
        if (bgmSource.clip == clip) return; // 이미 재생 중이면 넘어감

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


    // SFX그룹: 효과음
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        // 효과음은 중첩될 수 있으므로 PlayOneShot 사용
        sfxSource.PlayOneShot(clip); 
    }
    
    // 볼륨 조절 & 저장 (믹서 연동)
    public void SetBGMVolume(float volume) // 슬라이더 값 (0.0001 ~ 1.0)
    {
 
        // 믹서 볼륨 조절 (로그 스케일 요소 사용)
        float db = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20; 
        mainMixer.SetFloat(BGM_PARAM, db);
        
        // DataManager에 저장
        if (DataManager.Instance != null)
        {
            DataManager.Instance.currentData.bgmVolume = volume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        float db = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20;
        mainMixer.SetFloat(SFX_PARAM, db);

        if (DataManager.Instance != null)
        {
            DataManager.Instance.currentData.sfxVolume = volume;
        }
    }

    // 초기 실행 시 저장된 볼륨 적용
    public void LoadVolumeSettings()
    {
        if (DataManager.Instance == null) return;

    // DataManager에서 값 가져오기
        float bgmVol = DataManager.Instance.currentData.bgmVolume;
        float sfxVol = DataManager.Instance.currentData.sfxVolume;

        // 믹서에 적용 (Set 함수 재사용 시 저장 로직이 중복되는 문제가 있어 직접 설정)
        float bgmDb = Mathf.Log10(bgmVol) * 20;
        if (bgmVol <= 0.0001f) bgmDb = -80f;
        mainMixer.SetFloat("BGM", bgmDb);

        float sfxDb = Mathf.Log10(sfxVol) * 20;
        if (sfxVol <= 0.0001f) sfxDb = -80f;
        mainMixer.SetFloat("SFX", sfxDb);
    }
}