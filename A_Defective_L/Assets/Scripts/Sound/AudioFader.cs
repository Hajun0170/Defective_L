using System.Collections;
using UnityEngine;
using UnityEngine.Audio; 

public class AudioFader : MonoBehaviour //로드나 맵 이동 후 사운드 버그가 발생하여 일시적으로 안 들리게 처리
{
    [Header("Settings")]
    public AudioMixer audioMixer; // 오디오 믹서 연결
    public string parameterName = "SFX"; //SFX 효과음
    public float fadeDuration = 1.0f; // 소리가 커지는 데 걸리는 시간

    private void Start()
    {
        // 시작하자마자 소리 끄기 (-80dB == 완전 무음)

        bool result = audioMixer.SetFloat(parameterName, -80f);
        
        if (result)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            Debug.LogError($"오디오 믹서 파라미터 '{parameterName}'을 찾을 수 없으니 Exposed 설정을 확인.");
        }
    }

    IEnumerator FadeIn()
    {
        float currentTime = 0f;
        float startVolume = -80f;
        
        // 0dB(기본 최대)로 설정
        float targetVolume = 0f; 
        
        if (DataManager.Instance != null)
        {
            // 예시: targetVolume = Mathf.Log10(DataManager.Instance.currentData.sfxVolume) * 20;
        }

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            // Lerp로 서서히 볼륨 올림
            float newVol = Mathf.Lerp(startVolume, targetVolume, currentTime / fadeDuration);
            
            audioMixer.SetFloat(parameterName, newVol);
            yield return null;
        }

        // 확실하게 목표 값으로 고정
        audioMixer.SetFloat(parameterName, targetVolume);
    }
}