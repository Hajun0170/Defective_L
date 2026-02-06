using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DemoEndTrigger : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject endScreenPanel; // 검은 배경 + 텍스트가 있는 패널
    public CanvasGroup uiCanvasGroup; // 페이드 효과를 위해 패널에 CanvasGroup 컴포넌트 추가 권장

    [Header("설정")]
    public string titleSceneName = "Title"; // 타이틀 씬 이름
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

        // 1. 플레이어 조작 정지 (선택 사항)
        // 플레이어 움직임을 멈추고 싶다면 아래 주석 해제 (PlayerController가 있다고 가정)
        // player.GetComponent<PlayerController>().enabled = false;
        // player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // 2. UI 패널 켜기
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(true);

            // 서서히 어두워지는 효과 (CanvasGroup이 있다면)
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

        // 3. 문구 보여주며 대기 (페이드 시간 포함해서 기다릴지, 페이드 끝나고 기다릴지 결정)
        // 여기서는 페이드가 끝난 후 waitSeconds만큼 더 기다립니다.
        yield return new WaitForSeconds(waitSeconds);

        // 4. 타이틀로 이동
        // GameManager가 있다면 ChangeStage를 쓰고, 없다면 SceneManager를 씁니다.
        if (GameManager.Instance != null)
        {
            // 타이틀로 갈 때는 상태 초기화가 필요할 수 있으니
            // 필요하다면 GameManager.Instance.ResetGameData(); 같은 게 있으면 좋음
            GameManager.Instance.ChangeStage(titleSceneName);
        }
        else
        {
            SceneManager.LoadScene(titleSceneName);
        }
    }
}