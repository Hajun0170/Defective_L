using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    public CanvasGroup fadeCanvasGroup; // FadeImage에 붙은 Canvas Group 연결
    public float fadeDuration = 0.5f;   // 암전 시간

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동해도 파괴 X
        }
        else Destroy(gameObject);
    }

    // ★ 외부에서 부르는 함수
    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        // 1. 페이드 아웃 (점점 어두워짐)
        yield return StartCoroutine(Fade(1));

        // 2. 씬 로딩
        SceneManager.LoadScene(sceneName);

        // 3. 로딩 대기 (안정성)
        yield return new WaitForSeconds(0.1f);

        // 4. 페이드 인 (점점 밝아짐)
        yield return StartCoroutine(Fade(0));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        fadeCanvasGroup.blocksRaycasts = true; // 클릭 방지
        float speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / fadeDuration;

        while (!Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha))
        {
            fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
            yield return null;
        }
        fadeCanvasGroup.alpha = targetAlpha;
        if (targetAlpha == 0) fadeCanvasGroup.blocksRaycasts = false; // 클릭 허용
    }
}