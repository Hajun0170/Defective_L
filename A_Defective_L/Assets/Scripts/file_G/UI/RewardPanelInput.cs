using UnityEngine;
using System.Collections;
public class RewardPanelInput : MonoBehaviour //보상 패널. 키 조건을 일부 수정하기
{   
    private bool canClose = false; // 닫기 가능  (처음 false)
    // 패널이 활성화될 때마다 호출
    private void OnEnable()
    {
        // 게임 시간을 멈추고 싶다면
        Time.timeScale = 0f; 

        // 입력 잠금 (바로 닫히는 것 방지)
        canClose = false;

        // 1초 뒤에 잠금 해제하는 코루틴 시작
        StartCoroutine(EnableInputDelay());
    }

    private IEnumerator EnableInputDelay()
    {
        // TimeScale이 0이므로 WaitForSecondsRealtime을 써야 흐름
        yield return new WaitForSecondsRealtime(1.0f); 
        canClose = true; // 닫음
    }

    private void Update()
    {
        // canClose가 true일 때만 입력을 받음
        if (canClose && Input.anyKeyDown)
        {
            ClosePanel();
        }
    }
    
    void ClosePanel()
    {
        // 게임 시간 재개
        Time.timeScale = 1f;

        // 패널 끄기
        gameObject.SetActive(false);
    }
}