using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections.Generic;

public class SettingsUI : MonoBehaviour
{
    [Header("Video UI")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown screenModeDropdown; 
    
    public Slider bgmSlider;
    public Slider sfxSlider;
    // ★ [핵심 방어막] 코드가 UI를 맞추는 중인지 확인하는 변수
    private bool isSyncing = false; 

    private void Awake()
    {
        // 1. 드롭다운 글씨(Option A...) 세팅
        SetupDropdownOptions();

        // 2. UI 이벤트 연결 (Awake에서 최초 1번만 연결합니다)
        if(resolutionDropdown != null) 
            resolutionDropdown.onValueChanged.AddListener(OnVideoChanged);
        if(screenModeDropdown != null) 
            screenModeDropdown.onValueChanged.AddListener(OnVideoChanged);

        // ★ [중요] AudioManager를 직접 호출하도록 변경
        if(bgmSlider != null) 
            bgmSlider.onValueChanged.AddListener(v => {
                if (!isSyncing && AudioManager.Instance != null) 
                    AudioManager.Instance.SetVolume("BGM", v);
            });
            
        if(sfxSlider != null) 
            sfxSlider.onValueChanged.AddListener(v => {
                if (!isSyncing && AudioManager.Instance != null) 
                    AudioManager.Instance.SetVolume("SFX", v);
            });

    }// SettingsUI.cs 수정
private void OnEnable()
{
    // 씬이 바뀌어도 SettingsManager가 있으면 거기서 값을 가져오고, 
    // 없으면 PlayerPrefs에서 직접 가져옵니다.
    SyncUIWithSettings();
}

    private void SetupDropdownOptions()
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(new List<string> { "1920 x 1080", "1280 x 720" });
        }

        if (screenModeDropdown != null)
        {
            screenModeDropdown.ClearOptions();
            screenModeDropdown.AddOptions(new List<string> { "전체 화면", "테두리 없음", "창 모드" });
        }
    }

   private void SyncUIWithSettings()
{
    if (isSyncing) return;
    isSyncing = true; 

    // 저장된 값이 없으면 0f(기본값)를 가져옴
    float bgm = PlayerPrefs.GetFloat("BGM", 0.5f);
    float sfx = PlayerPrefs.GetFloat("SFX", 0.5f);

    Debug.Log($"[사운드 체크] 불러온 BGM: {bgm}");
    
    if(bgmSlider != null) bgmSlider.value = bgm;
    if(sfxSlider != null) sfxSlider.value = sfx;

    // 해상도/화면모드도 동일하게 처리
    if(resolutionDropdown != null) 
        resolutionDropdown.value = PlayerPrefs.GetInt("ResIndex", 0);
    if(screenModeDropdown != null) 
        screenModeDropdown.value = PlayerPrefs.GetInt("ScreenMode", 0);

    if(resolutionDropdown != null) resolutionDropdown.RefreshShownValue();
    if(screenModeDropdown != null) screenModeDropdown.RefreshShownValue();

    isSyncing = false; 
}

    private void OnVideoChanged(int dummyValue)
    {
        // 코드가 맞추는 중이면 무시
        if (isSyncing) return; 

        int resIdx = resolutionDropdown != null ? resolutionDropdown.value : 0;
        int modeIdx = screenModeDropdown != null ? screenModeDropdown.value : 0;
        
       // 해상도 설정은 간단하므로 여기서 직접 처리하거나 AudioManager에 넣어도 됩니다.
        SetResolution(resIdx, modeIdx);
    }

    // SettingsManager가 하던 해상도 기능을 이쪽으로 옮기거나 전용 매니저를 만드세요.
    private void SetResolution(int resIdx, int modeIdx)
    {
        Vector2Int[] res = { new Vector2Int(1920, 1080), new Vector2Int(1280, 720) };
        FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;
        if (modeIdx == 1) mode = FullScreenMode.FullScreenWindow;
        else if (modeIdx == 2) mode = FullScreenMode.Windowed;

        Screen.SetResolution(res[resIdx].x, res[resIdx].y, mode);
        
        PlayerPrefs.SetInt("ResIndex", resIdx);
        PlayerPrefs.SetInt("ScreenMode", modeIdx);
        PlayerPrefs.Save();
    }
}