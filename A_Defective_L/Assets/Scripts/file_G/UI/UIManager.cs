using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio; //오디오 믹서 사용을 위해 추가
using TMPro; 
using System.Collections;
using System.Collections.Generic; // 리스트 사용을 위해 추가

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Groups")]
    public GameObject hudGroup;      // 체력바
    public GameObject pauseGroup;    // 일시정지 메뉴 (Pause_Group)
    public GameObject optionsPopup;  // 옵션

    [Header("Health UI")]
    [SerializeField] private Image[] hpCells;
    [SerializeField] private Color hpFullColor = Color.red;
    [SerializeField] private Color hpEmptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

    [Header("Resource UI")]
    [SerializeField] private GameObject[] gaugeCells;
    [SerializeField] private GameObject[] ticketIcons;

    [Header("Weapon UI - 부모 그룹")]
    public GameObject meleeSlotGroup;
    public GameObject rangedSlotGroup;
    public GameObject ticketInfoGroup;

    [Header("Weapon UI - 자식 그룹 요소들")]
    public Image meleeIcon;
    public Image rangedIcon;

    [Header("Fade Effect")]
    public CanvasGroup fadeCanvasGroup;
    public Image fadeImage; // 색깔을 바꾸기 위해 Image 컴포넌트 필요
    public float fadeDuration = 0.5f;

    [Header("Boss UI")]
    public GameObject bossHudGroup;  // 아까 만든 Boss_HUD 연결
    public Slider bossHpSlider;      // 그 안의 Slider 연결

    [Header("Panels")]
    //public GameObject bossHUDPanel;
    public GameObject upgradePanel; // Scene_Gauge의 강화 패널 연결

    [Header("Potion UI")]
    public Image[] potionSlots; // 위에서 만든 3개의 이미지(Potion_1, 2, 3) 연결

   [Header("Health UI - Hybrid")]
    // 추가로 늘어나는 배경 오브젝트 (체력 슬롯 배경)
    // 기본 5칸 배경은 그냥 씬에 킴
    public GameObject[] extraHealthSlots;
    // 빨간 하트 이미지 (Fill_01 ~ Fill_07)
    // 묶인 배경 + 추가 획득 생명 하트
    public Image[] healthFills;

    [Header("Reward UI")]
    public GameObject rewardPanel;    // 패널 전체
    public UnityEngine.UI.Image rewardIcon; // 아이콘 이미지
    public TMPro.TMP_Text rewardName; // 이름 텍스트
    
    [Header("Key Hints Options")]
    // 인스펙터의 키 도움말 이미지 (공격키 아이콘, 점프키 아이콘 등)
    public GameObject[] keyHintObjects; 

    // 옵션 UI에 있는 체크박스
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
        // 게임 시작 시 저장된 설정대로 초기화
        if (DataManager.Instance != null)
        {
            bool isShown = DataManager.Instance.currentData.showKeyHints;
            ApplyKeyHintSetting(isShown);

            // 옵션 창의 체크박스 상태도 데이터와 맞춰줌
            if (keyHintToggle != null) 
            {
                keyHintToggle.isOn = isShown;
                
                // 토글에 이벤트 리스너 자동 연결 (클릭 시 함수 호출)
                keyHintToggle.onValueChanged.AddListener(OnKeyHintToggleChanged);
            }
        }
    }

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

    // 일시정지 및 옵션 기능

    // 일시정지 창 켜기/끄기
    public void TogglePauseUI(bool isPaused)
    {
        if (pauseGroup != null)
        {
            pauseGroup.SetActive(isPaused);
            
            // 일시정지 중엔 HUD 가리기 
            if (hudGroup != null) hudGroup.SetActive(!isPaused);
            
            // 옵션 창은 닫기
            if (optionsPopup != null) optionsPopup.SetActive(false);
        }
    }

    // 버튼 연결용 함수들
    public void OnClickResume()
    {
        if (GameManager.Instance != null) GameManager.Instance.TogglePause();
    }

    // UIManager.cs의 OnClickOptions 수정
public void OnClickOptions()
{
    if(pauseGroup != null) pauseGroup.SetActive(false);
    if(optionsPopup != null) 
    {
        optionsPopup.SetActive(true); 
        // optionsPopup이 켜지면서 그 자식에 붙은 SettingsUI의 OnEnable()이 실행
    }
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
            GameManager.Instance.ChangeStage("Title"); // 타이틀 씬으로 이동
            SetHUDActive(false);
            if(pauseGroup != null) pauseGroup.SetActive(false);
        }
    }


    // 페이드 색깔 바꾸기 (검정 <-> 흰색)
    public void SetFadeColor(Color color)
    {
        if (fadeImage != null)
        {
            fadeImage.color = color;
        }
    }

    // 보스 UI 켜기/끄기
    public void SetBossHUDActive(bool isActive)
    {
        if (bossHudGroup != null) bossHudGroup.SetActive(isActive);
    }

    // 보스 체력 갱신
    public void UpdateBossHealth(int current, int max)
    {
        if (bossHpSlider != null)
        {
            bossHpSlider.maxValue = max;
            bossHpSlider.value = current;
        }
    }

    // 강화창 열기/닫기 함수
    public void SetUpgradePanelActive(bool isActive)
    {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(isActive);
            
        }
    }
    
    // 강화창이 지금 켜져 있는지 확인용
    public bool IsUpgradePanelActive()
    {
        return upgradePanel != null && upgradePanel.activeSelf;
    }

    // 포션 UI 갱신 함수
    public void UpdatePotionUI(int current, int max)     // max: 해금된 최대 개수 (아이템 먹으면 늘어남)
    {
        for (int i = 0; i < potionSlots.Length; i++)
        {
            if (i < current) // current: 현재 남은 개수
            {
                // 사용 가능
                potionSlots[i].gameObject.SetActive(true);
                potionSlots[i].color = Color.white; // (투명도 원상복구)
            }
            else
            {
                // 사용함 OR 아직 해금 안 됨
                // 당장 사용 못 하는 건 눈앞에서 치움
                potionSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        //체력 (Fill) 처리 : 7개 

        for (int i = 0; i < healthFills.Length; i++)
        {
            // 체력 이미지를 보여줄지 결정 (최대 체력 범위 안인지 확인)
            if (i < maxHealth)
            {
                healthFills[i].gameObject.SetActive(true); // 하트 오브젝트 자체를 켬

                // 켜졌다면 꽉 채울지 비울지 결정 (현재 체력 기준)
                if (i < currentHealth)
                {
                    // 꽉 찬 체력 이미지 (투명도 100% or Sprite 교체)
                    healthFills[i].color = Color.white; 
                }
                else
                {
                    // 빈 체력 이미지 (투명하게 만들어서 배경만 보이게 함)
                    healthFills[i].color = new Color(1, 1, 1, 0); 
                }
            }
            else
            {
                // 최대 체력 밖의 이미지는 아예 끔.
                healthFills[i].gameObject.SetActive(false);
            }
        }

        // 추가 배경 (Slot) 처리 : 2개
        // 기본 5칸은 이미 깔려있다고 가정, 6번째(인덱스 0)부터 계산합니다.
        int baseHealthCount = 5; // 기본으로 깔려있는 배경 개수

        for (int i = 0; i < extraHealthSlots.Length; i++)
        {
            // i=0 (Slot_06) -> maxHealth가 6 이상이면 켜짐
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

    // 보상 패널 띄우기 함수
    public void ShowRewardPanel(Weapon weapon)
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
            
            // UI 내용 채우기
            if (rewardIcon != null) rewardIcon.sprite = weapon.icon;
            if (rewardName != null) rewardName.text = $"{weapon.weaponName} 획득!";
            
            // 보상 받을 때 게임 일시정지
            Time.timeScale = 0; 
        }
    }

    // 확인 버튼에 연결할 함수 (패널 닫기)
    public void CloseRewardPanel()
    {
        if (rewardPanel != null) rewardPanel.SetActive(false);
        Time.timeScale = 1; // 게임 재개
    }
    
    // 체크박스름 눌렀을 때 호출될 함수
    public void OnKeyHintToggleChanged(bool isOn)
    {
        // 데이터 변경 및 저장
        if (DataManager.Instance != null)
        {
            DataManager.Instance.currentData.showKeyHints = isOn;
            DataManager.Instance.SaveDataToDisk();
        }

        // 화면 갱신
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