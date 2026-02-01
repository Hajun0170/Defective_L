using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    // 인스펙터에서 "Prologue" 또는 "Stage1" 이라고 정확히 적어주세요!
    [SerializeField] private string nextSceneName = "Prologue"; 

    private void Start()
    {
        // 1. 매니저에게 "지금 컷씬 켜져 있어(이거 나중에 꺼야 해)"라고 알려줌
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentStageName = SceneManager.GetActiveScene().name;
        }

        // 2. 컷씬 보는 동안 체력바(HUD) 숨기기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHUDActive(false);
        }

        // 3. 타이머 시작
        StartCoroutine(WaitAndMove());
    }

    IEnumerator WaitAndMove()
    {
        // 10초 대기 (연출 시간)
        yield return new WaitForSeconds(10.0f);

        // ★ [핵심] GameManager를 통해 다음 스테이지로 안전하게 이동
        if (GameManager.Instance != null)
        {
            // 1. 데이터에 "다음 씬은 프롤로그다"라고 기록
            DataManager.Instance.currentData.sceneName = nextSceneName;

            // 2. 위치는 프롤로그의 시작점(예: 0,0)으로 초기화 (안 하면 엉뚱한 곳에 낌)
            DataManager.Instance.currentData.playerX = -17f; //문제 있을 수 있으니 보류
            DataManager.Instance.currentData.playerY = -3.5f;

            // 3. 파일로 덮어쓰기! (이제 죽어서 로드하면 여기부터 시작됨)
            DataManager.Instance.SaveDataToDisk();

            // 다음 맵으로 이동 (Prologue 등)
            GameManager.Instance.ChangeStage(nextSceneName);
            
            // 이동 후 HUD 다시 켜기
           // if (UIManager.Instance != null) 
               // UIManager.Instance.SetHUDActive(true);
        }
        else
        {
            // 비상용 (매니저 없으면 강제 로드)
            SceneManager.LoadScene(nextSceneName);
        }
    }
}