using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio; // ★ 오디오 믹서 사용을 위해 추가
using TMPro; // ★ 이 줄을 꼭 추가해야 합니다! (TextMeshPro 사용 선언)
using System.Collections;
using System.Collections.Generic; // ★ 리스트 사용을 위해 추가

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Groups")]
    public GameObject hudGroup;      // HUD (체력바 등)
    public GameObject pauseGroup;    // ★ [추가] 일시정지 메뉴 (Pause_Group)
    public GameObject optionsPopup;  // ★ [추가] 옵션 팝업 (Option)

    [Header("1. Health UI")]
    [SerializeField] private Image[] hpCells;
    [SerializeField] private Color hpFullColor = Color.red;
    [SerializeField] private Color hpEmptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    [Header("2. Resource UI")]
    [SerializeField] private GameObject[] gaugeCells;
    [SerializeField] private GameObject[] ticketIcons;

    [Header("3. Weapon UI - Parents")]
    public GameObject meleeSlotGroup;
    public GameObject rangedSlotGroup;
    public GameObject ticketInfoGroup;

    [Header("4. Weapon UI - Children")]
    public Image meleeIcon;
    public Image rangedIcon;

    [Header("Fade Effect")]
    public CanvasGroup fadeCanvasGroup;
    public Image fadeImage; // ★ [추가] 색깔을 바꾸기 위해 Image 컴포넌트 필요
    public float fadeDuration = 0.5f;

    [Header("★ Audio Settings")]
    public AudioMixer mainMixer;     // 오디오 믹서 연결
    public Slider bgmSlider;         // BGM 슬라이더 연결
    public Slider sfxSlider;         // SFX 슬라이더 연결

    [Header("★ Video Settings")]
  // 기존: public Dropdown resolutionDropdown; 
    // ▼ 아래처럼 바꾸세요 ▼
    public TMP_Dropdown resolutionDropdown; 
    
    // 기존: public Dropdown screenModeDropdown;
    // ▼ 아래처럼 바꾸세요 ▼
    public TMP_Dropdown screenModeDropdown;

    [Header("★ Boss UI")]
    public GameObject bossHudGroup;  // 아까 만든 Boss_HUD 연결
    public Slider bossHpSlider;      // 그 안의 Slider 연결

    [Header("Panels")]
    //public GameObject bossHUDPanel;
    public GameObject upgradePanel; // ★ 여기에 Scene_Gauge의 강화 패널 연결

    [Header("Potion UI")]
    public Image[] potionSlots; // 위에서 만든 3개의 이미지(Potion_1, 2, 3) 연결

   [Header("Health UI - Hybrid")]
    // ★ [수정] 여기에는 '추가로 늘어나는 배경 오브젝트'만 넣으세요! (Slot_06, Slot_07)
    // 기본 5칸 배경은 그냥 씬에 켜두시면 됩니다. (코드에서 안 건드림)
    public GameObject[] extraHealthSlots;
    
   // ★ [유지] 여기에는 빨간 하트 이미지 7개를 전부 순서대로 넣으세요! (Fill_01 ~ Fill_07)
    // 배경이 묶여있더라도, 체력바 역할을 하는 하트 이미지는 7개가 따로 있어야 합니다.
    public Image[] healthFills;

    [Header("Reward UI")]
    public GameObject rewardPanel;    // 패널 전체
    public UnityEngine.UI.Image rewardIcon; // 아이콘 이미지
    public TMPro.TMP_Text rewardName; // 이름 텍스트
    
    [Header("Key Hints Options")]
    // ★ 인스펙터에서 키 도움말 이미지들을 싹 다 넣으세요 (공격키 아이콘, 점프키 아이콘 등)
    // 혹은 부모 오브젝트 하나만 넣어도 됩니다.
    public GameObject[] keyHintObjects; 

    // 옵션 UI에 있는 토글(체크박스)도 연결 (옵션창 열 때 현재 상태 반영용)
    public UnityEngine.UI.Toggle keyHintToggle;

    // 내부 변수들
    List<Resolution> resolutions = new List<Resolution>();
    FullScreenMode[] screenModes = { FullScreenMode.ExclusiveFullScreen, FullScreenMode.FullScreenWindow, FullScreenMode.Windowed };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    

    private void Start()
    {
        InitVideoSettings(); // ★ 시작할 때 해상도 목록 채우기
        InitAudioUI();       // ★ 슬라이더 위치 초기화

        // 게임 시작 시 저장된 설정대로 초기화
        if (DataManager.Instance != null)
        {
            bool isShown = DataManager.Instance.currentData.showKeyHints;
            ApplyKeyHintSetting(isShown);

            // 옵션 창의 체크박스 상태도 데이터와 맞춰줌
            if (keyHintToggle != null) 
            {
                keyHintToggle.isOn = isShown;
                
                // ★ 토글에 이벤트 리스너 자동 연결 (클릭 시 함수 호출)
                keyHintToggle.onValueChanged.AddListener(OnKeyHintToggleChanged);
            }
        }
    }

    // ====================================================
    // ▼ 기존 기능 함수들 (그대로 유지)
    // ====================================================

    public void UpdateHealth(int currentHealth)
    {
        for (int i = 0; i < hpCells.Length; i++)
        {
            hpCells[i].color = (i < currentHealth) ? hpFullColor : hpEmptyColor;
            hpCells[i].gameObject.SetActive(true);
        }
    }

    public void UpdateGauge(int current, int max)
    {
        for (int i = 0; i < gaugeCells.Length; i++)
        {
            gaugeCells[i].SetActive(i < current);
        }
    }

    public void UpdateTickets(int count)
    {
        for (int i = 0; i < ticketIcons.Length; i++)
        {
            ticketIcons[i].SetActive(i < count);
        }
    }

    public void SetSlotVisibility(bool showMelee, bool showRanged)
    {
        if (meleeSlotGroup != null) meleeSlotGroup.SetActive(showMelee);
        if (rangedSlotGroup != null) rangedSlotGroup.SetActive(showRanged);
        if (ticketInfoGroup != null) ticketInfoGroup.SetActive(showMelee || showRanged);
    }

    public void UpdateWeaponSlots(Weapon melee, Weapon ranged)
    {
        if (meleeIcon == null || rangedIcon == null) return;

        if (melee != null)
        {
            meleeIcon.sprite = melee.icon;
            meleeIcon.gameObject.SetActive(true);
        }

        if (ranged != null)
        {
            rangedIcon.sprite = ranged.icon;
            rangedIcon.gameObject.SetActive(true);
        }
    }

    public IEnumerator FadeOut()
    {
        if (fadeCanvasGroup == null) yield break;
        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.blocksRaycasts = true;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            fadeCanvasGroup.alpha = t;
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
    }

    public IEnumerator FadeIn()
    {
        if (fadeCanvasGroup == null) yield break;

        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime / fadeDuration;
            fadeCanvasGroup.alpha = t;
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.gameObject.SetActive(false);
    }

    public void SetHUDActive(bool isActive)
    {
        if (hudGroup != null) hudGroup.SetActive(isActive);
    }

    // ====================================================
    // ▼ [새로 추가] 일시정지 및 옵션 기능
    // ====================================================

    // 1. 일시정지 창 켜기/끄기
    public void TogglePauseUI(bool isPaused)
    {
        if (pauseGroup != null)
        {
            pauseGroup.SetActive(isPaused);
            
            // 일시정지 중엔 HUD 가리기 (선택사항)
            if (hudGroup != null) hudGroup.SetActive(!isPaused);
            
            // 옵션 창은 닫기
            if (optionsPopup != null) optionsPopup.SetActive(false);
        }
    }

    // 2. 버튼 연결용 함수들
    public void OnClickResume()
    {
        if (GameManager.Instance != null) GameManager.Instance.TogglePause();
    }

    public void OnClickOptions()
    {
        if(pauseGroup != null) pauseGroup.SetActive(false);
        if(optionsPopup != null) 
        {
            optionsPopup.SetActive(true);

            // ★ [추가] 옵션창 열 때 슬라이더 위치 재동기화 (Sync)
            SyncAudioSliders();
        }
    }

    // 슬라이더 동기화용 헬퍼 함수
    void SyncAudioSliders()
    {
        // 저장된 값 다시 읽어오기
        float savedBGM = PlayerPrefs.GetFloat("BGM_Volume", 1.0f);
        float savedSFX = PlayerPrefs.GetFloat("SFX_Volume", 1.0f);

        // 슬라이더에 반영 (이벤트 트리거 없이 값만 변경하고 싶다면 SetValueWithoutNotify 사용 가능하지만, 
        // 여기선 그냥 value 대입해도 괜찮습니다. 어차피 같은 값이라 소리 변화 없음)
        if (bgmSlider != null) bgmSlider.value = savedBGM;
        if (sfxSlider != null) sfxSlider.value = savedSFX;
        
        // 해상도 UI도 같이 동기화하고 싶다면?
        // if (resolutionDropdown != null) resolutionDropdown.value = PlayerPrefs.GetInt("Resolution_Index", 0);
        // if (screenModeDropdown != null) screenModeDropdown.value = PlayerPrefs.GetInt("Screen_Mode", 0);
    }

    public void OnClickCloseOptions()
    {
        if(optionsPopup != null) optionsPopup.SetActive(false);
        if(pauseGroup != null) pauseGroup.SetActive(true);
    }

    public void OnClickToTitle()
    {
        Time.timeScale = 1f; // 시간 다시 흐르게
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isPaused = false; // 상태 초기화
            GameManager.Instance.ChangeStage("Title"); // 타이틀 씬 이름 확인!
            SetHUDActive(false);
            if(pauseGroup != null) pauseGroup.SetActive(false);
        }
    }

    // 3. 오디오 설정
    public void SetBGMVolume(float volume)
    {
        if(mainMixer != null) mainMixer.SetFloat("BGM", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (mainMixer != null) mainMixer.SetFloat("SFX", volume);
    }

    // 4. 비디오 설정
   // 4. 비디오 설정 (수정됨: 고정 해상도 사용)
    void InitVideoSettings()
    {
        // -------------------------------------------------------
        // A. 해상도 초기화 (1920x1080, 1280x720 고정)
        // -------------------------------------------------------
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            resolutions.Clear();

            List<string> options = new List<string>();

            // 1. FHD (1920 x 1080) 추가
            Resolution r1 = new Resolution();
            r1.width = 1920; 
            r1.height = 1080;
            resolutions.Add(r1);
            options.Add("1920 x 1080");

            // 2. HD (1280 x 720) 추가
            Resolution r2 = new Resolution();
            r2.width = 1280; 
            r2.height = 720;
            resolutions.Add(r2);
            options.Add("1280 x 720");

            resolutionDropdown.AddOptions(options);

            // ★ 저장된 해상도 불러오기 (없으면 0번: 1920x1080 기본)
            int savedResIdx = PlayerPrefs.GetInt("Resolution_Index", 0);
            
            // 인덱스 안전 장치 (혹시 목록보다 큰 값이 저장되어 있을까봐)
            if (savedResIdx >= resolutions.Count) savedResIdx = 0;

            resolutionDropdown.value = savedResIdx;
            resolutionDropdown.RefreshShownValue();

            // 실제로 적용도 한번 해줍니다 (게임 켜자마자 적용되게)
            SetResolution(savedResIdx);
        }

        // -------------------------------------------------------
        // B. 화면모드 초기화 (전체, 테두리 없음, 창모드)
        // -------------------------------------------------------
        if (screenModeDropdown != null)
        {
            screenModeDropdown.ClearOptions();
            List<string> modeOptions = new List<string> { "전체 화면", "테두리 없음", "창 모드" };
            screenModeDropdown.AddOptions(modeOptions);

            // ★ 저장된 화면모드 불러오기 (없으면 0번: 전체화면 기본)
            int savedModeIdx = PlayerPrefs.GetInt("Screen_Mode", 0);
            
            screenModeDropdown.value = savedModeIdx;
            screenModeDropdown.RefreshShownValue();

            // 실제로 적용
            SetScreenMode(savedModeIdx);
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions.Count > resolutionIndex)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);

            // ★ 변경 시 저장
            PlayerPrefs.SetInt("Resolution_Index", resolutionIndex);
            PlayerPrefs.Save();
        }
    }

    public void SetScreenMode(int modeIndex)
    {
        Screen.SetResolution(Screen.width, Screen.height, screenModes[modeIndex]);
    }

    // ★ [추가] 페이드 색깔 바꾸기 (검정 <-> 흰색)
    public void SetFadeColor(Color color)
    {
        if (fadeImage != null)
        {
            fadeImage.color = color;
        }
    }

    // 1. 보스 UI 켜기/끄기
    public void SetBossHUDActive(bool isActive)
    {
        if (bossHudGroup != null) bossHudGroup.SetActive(isActive);
    }

    // 2. 보스 체력 갱신
    public void UpdateBossHealth(int current, int max)
    {
        if (bossHpSlider != null)
        {
            bossHpSlider.maxValue = max;
            bossHpSlider.value = current;
        }
    }

    // ★ 강화창 열기/닫기 함수 추가
    public void SetUpgradePanelActive(bool isActive)
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(isActive);
            
            // (선택) 강화창 켰을 때 게임 일시정지 하려면?
            // Time.timeScale = isActive ? 0 : 1; 
        }
    }
    
    // 강화창이 지금 켜져 있는지 확인용
    public bool IsUpgradePanelActive()
    {
        return upgradePanel != null && upgradePanel.activeSelf;
    }

    // ★ [추가] 포션 UI 갱신 함수
   // current: 현재 남은 개수
    // max: 해금된 최대 개수 (아이템 먹으면 늘어남)
   // ★ [수정됨] 포션 UI 갱신 (이미지 교체 X -> 투명도 조절 O)
    public void UpdatePotionUI(int current, int max)
    {
        for (int i = 0; i < potionSlots.Length; i++)
        {
            if (i < current)
            {
                // [상태: 사용 가능]
                potionSlots[i].gameObject.SetActive(true);
                potionSlots[i].color = Color.white; // (혹시 투명도 건드렸을까봐 원상복구)
            }
            else
            {
                // [상태: 사용함 OR 아직 해금 안 됨]
                // 썼든 안 배웠든, 당장 못 쓰는 건 눈앞에서 치워버립니다.
                potionSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        // --------------------------------------------------------
        // 1. 빨간 하트 (Fill) 처리 : 7개 전부 검사
        // --------------------------------------------------------
        for (int i = 0; i < healthFills.Length; i++)
        {
            // (1) 일단 이 하트를 보여줄지 결정 (최대 체력 범위 안인가?)
            if (i < maxHealth)
            {
                healthFills[i].gameObject.SetActive(true); // 하트 오브젝트 자체를 켬

                // (2) 켜졌다면, 꽉 채울지 비울지 결정 (현재 체력 기준)
                if (i < currentHealth)
                {
                    // 꽉 찬 하트 (투명도 100% or Sprite 교체)
                    healthFills[i].color = Color.white; 
                }
                else
                {
                    // 빈 하트 (투명하게 만들어서 배경만 보이게 함)
                    healthFills[i].color = new Color(1, 1, 1, 0); 
                }
            }
            else
            {
                // 최대 체력 밖의 하트는 아예 끕니다.
                healthFills[i].gameObject.SetActive(false);
            }
        }

        // --------------------------------------------------------
        // 2. 추가 배경 (Slot) 처리 : 추가된 2개만 검사
        // --------------------------------------------------------
        // 기본 5칸은 이미 깔려있다고 가정하므로, 6번째(인덱스 0)부터 계산합니다.
        int baseHealthCount = 5; // 기본으로 깔려있는 배경 개수

        for (int i = 0; i < extraHealthSlots.Length; i++)
        {
            // 예: i=0 (Slot_06) -> maxHealth가 6 이상이면 켜짐
            // 예: i=1 (Slot_07) -> maxHealth가 7 이상이면 켜짐
            if (baseHealthCount + (i + 1) <= maxHealth)
            {
                extraHealthSlots[i].SetActive(true);
            }
            else
            {
                extraHealthSlots[i].SetActive(false);
            }
        }
    }

    // 1. 오디오 UI 초기화 (저장된 값으로 슬라이더 맞추기)
    void InitAudioUI()
    {
        float savedBGM = PlayerPrefs.GetFloat("BGM_Volume", 1.0f);
        float savedSFX = PlayerPrefs.GetFloat("SFX_Volume", 1.0f);

        if (bgmSlider != null) 
        {
            bgmSlider.value = savedBGM;
            // 슬라이더 움직일 때 AudioManager 호출 연결
            bgmSlider.onValueChanged.AddListener((val) => AudioManager.Instance.SetBGMVolume(val));
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFX;
            sfxSlider.onValueChanged.AddListener((val) => AudioManager.Instance.SetSFXVolume(val));
        }
    }

    // 보상 패널 띄우기 함수
    public void ShowRewardPanel(Weapon weapon)
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
            
            // UI 내용 채우기
            if (rewardIcon != null) rewardIcon.sprite = weapon.icon;
            if (rewardName != null) rewardName.text = $"{weapon.weaponName} 획득!";
            
            // (선택) 보상 받을 때 게임 일시정지
            Time.timeScale = 0; 
        }
    }

    // 확인 버튼에 연결할 함수 (패널 닫기)
    public void CloseRewardPanel()
    {
        if (rewardPanel != null) rewardPanel.SetActive(false);
        Time.timeScale = 1; // 게임 재개
    }
    
    // ★ 토글(체크박스)을 눌렀을 때 호출될 함수
    public void OnKeyHintToggleChanged(bool isOn)
    {
        // 1. 데이터 변경 및 저장
        if (DataManager.Instance != null)
        {
            DataManager.Instance.currentData.showKeyHints = isOn;
            DataManager.Instance.SaveDataToDisk();
        }

        // 2. 화면 갱신
        ApplyKeyHintSetting(isOn);
    }

    // 실제 오브젝트를 껐다 켜는 함수
    private void ApplyKeyHintSetting(bool isShown)
    {
        if (keyHintObjects == null) return;

        foreach (GameObject obj in keyHintObjects)
        {
            if (obj != null) obj.SetActive(isShown);
        }
    }
}