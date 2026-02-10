using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour //UI 매니저에서 있던 기능을 옮김: 연동 문제와 매니저가 비대해진 문제
{
    public enum VolumeType { BGM, SFX }
    public VolumeType type; 

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        // DataManager가 없으면 타이틀부터 실행 안 한 것이라 무시
        if (DataManager.Instance == null) return;

        float savedVolume = 1f;

        if (type == VolumeType.BGM)
        {
            savedVolume = DataManager.Instance.currentData.bgmVolume;
        }
        else if (type == VolumeType.SFX)
        {
            savedVolume = DataManager.Instance.currentData.sfxVolume;
        }

        // 저장된 값으로 슬라이더 이동: 소리 재생 X
        if (slider != null)
        {
            slider.SetValueWithoutNotify(savedVolume);
        }
    }

    public void OnSliderChanged(float value)
    {
        if (AudioManager.Instance == null) return;

        if (type == VolumeType.BGM)
        {
            AudioManager.Instance.SetBGMVolume(value);
        }
        else
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }
}