using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneManager : MonoBehaviour //컷씬 재생용
{
    [SerializeField] private string nextSceneName = "Prologue"; //인스펙터에서 조정 가능

    private void Start()
    {
        // 매니저에게 컷씬 켜짐을 알려줌 (나중에 끄긴함)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentStageName = SceneManager.GetActiveScene().name;
        }

        // 컷씬 보는 동안 체력바(HUD) 숨기기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHUDActive(false);
        }

        // 타이머 시작
        StartCoroutine(WaitAndMove());
    }

    IEnumerator WaitAndMove()
    {
        // 10초 대기 (연출 시간), 이 사이에 만든 컷씬을 재생함
        yield return new WaitForSeconds(10.0f);

        // GameManager를 통해 다음 스테이지로 이동
        if (GameManager.Instance != null)
        {
            // 다음 씬은 프롤로그로 기억
            DataManager.Instance.currentData.sceneName = nextSceneName;

            //프롤로그 시작 위치, 값 건드리면 꼬일 가능성이 높음
            DataManager.Instance.currentData.playerX = -17f; //문제 있을 수 있으니 보류
            DataManager.Instance.currentData.playerY = -3.5f;

            // 파일로 덮어써서 죽어서 로드하면 여기부터 시작 지점이 리셋, 다른 코드에서 이걸 무시하고 프롤로그로 이동하는 부분 있음
            DataManager.Instance.SaveDataToDisk();

            // 다음 맵으로 이동 Prologue // 
            GameManager.Instance.ChangeStage(nextSceneName);
            
            // 이동 후 HUD 다시 켜기 : 다른 곳에 자동으로 키는 기능이 있어서 주석
           // if (UIManager.Instance != null) 
               // UIManager.Instance.SetHUDActive(true);
        }
        else
        {
            //매니저 없을 때 테스트 용도
            SceneManager.LoadScene(nextSceneName);
        }
    }
}