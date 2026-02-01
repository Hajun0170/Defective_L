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
        if(optionsPopup != null) optionsPopup.SetActive(true);
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
    void InitVideoSettings()
    {
        // 해상도 초기화
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            resolutions.Clear();
            List<string> options = new List<string>();
            int currentResIndex = 0;

            Resolution[] allResolutions = Screen.resolutions;
            for (int i = 0; i < allResolutions.Length; i++)
            {
                string option = allResolutions[i].width + " x " + allResolutions[i].height;
                resolutions.Add(allResolutions[i]);
                options.Add(option);

                if (allResolutions[i].width == Screen.width && allResolutions[i].height == Screen.height)
                    currentResIndex = i;
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResIndex;
            resolutionDropdown.RefreshShownValue();
        }

        // 화면모드 초기화
        if (screenModeDropdown != null)
        {
            screenModeDropdown.ClearOptions();
            List<string> modeOptions = new List<string> { "전체 화면", "테두리 없음", "창 모드" };
            screenModeDropdown.AddOptions(modeOptions);
            
            if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen) screenModeDropdown.value = 0;
            else if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) screenModeDropdown.value = 1;
            else screenModeDropdown.value = 2;
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions.Count > resolutionIndex)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
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
}