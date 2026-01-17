using UnityEngine;


public class TitleManager : MonoBehaviour
{
    // [새로 하기] 버튼용
    public void ClickNewGame()
    {
        // 1. 데이터 초기화
        DataManager.Instance.NewGame();

    // 매니저로 씬 연결
        SceneTransitionManager.Instance.LoadScene("Cutscene_Intro");
    }

    // [이어 하기] 버튼용
    public void ClickLoadGame()
    {
        // 1. 파일 불러오기 시도
        if (DataManager.Instance.LoadGame())
        {
           // [수정]
            SceneTransitionManager.Instance.LoadScene(DataManager.Instance.currentData.sceneName);
        }
        else
        {
            // 실패 시 (파일 없음) 소리나 흔들림 효과 등을 줄 수 있음
            Debug.Log("불러올 데이터가 없습니다.");
        }
    }

    // [나가기] 버튼용
    public void ClickExit()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }
}