using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour 
{
    public enum VolumeType { BGM, SFX }
    public VolumeType type; 

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        // 인스펙터 연결 대신 코드로 직접 이벤트를 추가하여 실수 방지
    slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnEnable()
    {
        // DataManager를 보지 않고 PlayerPrefs에서 직접 가져옴
        // 저장된 값이 없으면 0.5f를 기본값으로 사용
        float savedVolume = PlayerPrefs.GetFloat(type.ToString(), 0.5f);

        // 저장된 값으로 슬라이더 위치 세팅 (이벤트 호출 안 함)
        if (slider != null)
        {
            slider.SetValueWithoutNotify(savedVolume);
        }
    }

    // 슬라이더 조절 시 실행 (인스펙터의 OnValueChanged에 연결)
   public void OnSliderChanged(float value)
{
   if (AudioManager.Instance == null) return;

    // 공통 함수인 SetVolume을 사용하도록 통일 (매개변수 2개)
    AudioManager.Instance.SetVolume(type.ToString(), value);
}
}