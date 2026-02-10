using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;

public class TitleUIConnector : MonoBehaviour
{
    [Header("Window Control")]
    public GameObject optionWindowPanel; 

    [Header("Audio UI")]
    //public Slider bgmSlider;
    //public Slider sfxSlider;

    [Header("Screen UI")]
    public TMP_Dropdown resolutionDropdown;   // 해상도 
    public TMP_Dropdown screenModeDropdown;   // 화면모드 

    // 해상도 정보를 담을 리스트
    List<Resolution> resolutions = new List<Resolution>();

    private void Start()
    {
        // 옵션창 끄기
        if (optionWindowPanel != null) optionWindowPanel.SetActive(false);

        // 비디오 설정 초기화
        InitVideoSettings();
    }

    public void OpenOptions() => optionWindowPanel.SetActive(true);
    public void CloseOptions() => optionWindowPanel.SetActive(false);

    void InitVideoSettings()
    {
        // 해상도 초기화 1920x1080, 1280x720
 
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            resolutions.Clear();

            List<string> options = new List<string>();

            // 1920 x 1080
            Resolution r1 = new Resolution(); r1.width = 1920; r1.height = 1080;
            resolutions.Add(r1);
            options.Add("1920 x 1080");

            // 1280 x 720
            Resolution r2 = new Resolution(); r2.width = 1280; r2.height = 720;
            resolutions.Add(r2);
            options.Add("1280 x 720");

            resolutionDropdown.AddOptions(options);

            // 저장된 값 불러오기
            int savedResIdx = PlayerPrefs.GetInt("Resolution_Index", 0);
            if (savedResIdx >= resolutions.Count) savedResIdx = 0;

            resolutionDropdown.value = savedResIdx;
            resolutionDropdown.RefreshShownValue();
            
            // 이벤트 연결
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }

        // 화면모드 초기화: 전체, 테두리 없음, 창모드
 
        if (screenModeDropdown != null)
        {
            screenModeDropdown.ClearOptions();
            List<string> modeOptions = new List<string> { "전체 화면", "테두리 없음", "창 모드" };
            screenModeDropdown.AddOptions(modeOptions);

            // 저장된 값 불러오기
            int savedModeIdx = PlayerPrefs.GetInt("Screen_Mode", 0);
            
            screenModeDropdown.value = savedModeIdx;
            screenModeDropdown.RefreshShownValue();

            // 이벤트 연결
            screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
        }
    }

    //해상도 변경 함수
    public void SetResolution(int index)
    {
        if (index >= resolutions.Count) return;

        Resolution res = resolutions[index];
        
        // 현재 화면 모드 유지하면서 해상도만 변경
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);

        // 변경 사항 저장 (PlayerPrefs)
        PlayerPrefs.SetInt("Resolution_Index", index);
        PlayerPrefs.Save();

        Debug.Log($"현재 해상도 변경: {resolutions[index].width} x {resolutions[index].height}");
        
        // 코루틴으로 한 프레임 뒤에 찍어봄
        StartCoroutine(CheckResolutionNextFrame());
    }

    IEnumerator CheckResolutionNextFrame()
    {
        yield return null; // 한 프레임 대기 (적용 시간 필요)
        Debug.Log($"적용된 해상도: {Screen.width} x {Screen.height}");
    }

    // 화면 모드 변경 함수
    public void SetScreenMode(int index)
    {
        FullScreenMode mode = FullScreenMode.ExclusiveFullScreen;

        switch (index)
        {
            case 0: mode = FullScreenMode.ExclusiveFullScreen; break; // 전체 화면
            case 1: mode = FullScreenMode.FullScreenWindow; break;    // 테두리 없음
            case 2: mode = FullScreenMode.Windowed; break;            // 창 모드
        }

        Screen.fullScreenMode = mode;

        // 변경 사항 저장 PlayerPrefs
        PlayerPrefs.SetInt("Screen_Mode", index);
        PlayerPrefs.Save();
    }
}