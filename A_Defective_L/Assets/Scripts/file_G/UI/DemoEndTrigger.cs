using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DemoEndTrigger : MonoBehaviour //엔딩을 위해 띄운 트리거인데 변경 후 사용 안 함
{
    [Header("UI 연결")]
    public GameObject endScreenPanel; // 검은 배경, 텍스트가 있는 패널
    public CanvasGroup uiCanvasGroup; // 페이드 효과를 위해 패널에 CanvasGroup 컴포넌트 추가

    [Header("설정")]
    public string titleSceneName = "Title"; // 타이틀 씬
    public float waitSeconds = 3.0f;        // 대기 시간
    public float fadeDuration = 1.0f;       // 화면이 어두워지는 속도

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return; // 중복 실행 방지

        if (collision.CompareTag("Player"))
        {
            StartCoroutine(EndSequence(collision.gameObject));
        }
    }

    IEnumerator EndSequence(GameObject player)
    {
        isTriggered = true;

        //UI 패널 온
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(true);

            // 페이드 인 활성화
            if (uiCanvasGroup != null)
            {
                uiCanvasGroup.alpha = 0f;
                float timer = 0f;
                while (timer < fadeDuration)
                {
                    timer += Time.deltaTime;
                    uiCanvasGroup.alpha = timer / fadeDuration;
                    yield return null;
                }
                uiCanvasGroup.alpha = 1f;
            }
        }

        //문구 보여주며 대기 
        //페이드가 끝난 후 waitSeconds만큼 추가 대기
        yield return new WaitForSeconds(waitSeconds);

        // 타이틀로 이동
        if (GameManager.Instance != null)
        {
            // 타이틀로 갈 때는 상태 초기화가 필요
            GameManager.Instance.ChangeStage(titleSceneName);
        }
        else
        {
            SceneManager.LoadScene(titleSceneName);
        }
    }
}