using UnityEngine;
using UnityEngine.SceneManagement; 

public class TitleManager : MonoBehaviour //타이틀 화면 관리
{
   private void Start()
    {
        // UI 씬 로드 
        if (SceneManager.GetSceneByName("Scene_Gauge").isLoaded == false)
        {
             SceneManager.LoadScene("Scene_Gauge", LoadSceneMode.Additive);
        }

        // 타이틀. 체력바는 숨김
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHUDActive(false); 
        }

        // 매니저 설정
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentStageName = SceneManager.GetActiveScene().name;
        }
    }

    // 새로 하기 버튼
    public void ClickNewGame()
    {
        DataManager.Instance.NewGame();

        // 초기화된 데이터를 파일에 저장
        // 죽었을 때 로드하면 초기 상태가 불러와짐
        DataManager.Instance.SaveDataToDisk();

        if (GameManager.Instance != null)
        {
            //게임 시작: HUD 다시 킴
            GameManager.Instance.currentStageName = SceneManager.GetActiveScene().name;
            GameManager.Instance.ChangeStage("Cutscene_Intro");
        }
    }

    // [이어 하기] 버튼용
    public void ClickLoadGame()
    {
        if (DataManager.Instance.LoadGame())
        {
            if (GameManager.Instance != null)
            {   
                // 저장된 씬 이름 확인
                string savedScene = DataManager.Instance.currentData.sceneName;

                // 프롤로그, 컷신, 타이틀에서 저장된 데이터라면 이어하기 금지!
                if (savedScene == "Prologue" || savedScene == "Cutscene_Intro" || savedScene == "Title")
                {
                    Debug.Log("프롤로그 데이터는 초기화");

                    // 데이터 싹 비우고 새 게임 시작
                    DataManager.Instance.NewGame(); 

                    // NewGame 직후에 사운드 설정은 PlayerPrefs에서 다시 불러와서 믹서에 적용
                     if (AudioManager.Instance != null) AudioManager.Instance.LoadVolumeSettings();
                    
                    // 1스테이지로 강제 이동
                    GameManager.Instance.currentStageName = "Title"; // 현재 위치는 타이틀
                    GameManager.Instance.ChangeStage("Prologue");      // 목표는 1스테이지
                    
                    // HUD 켜기
                    if (UIManager.Instance != null) UIManager.Instance.SetHUDActive(true);
                    return; // 함수 종료
                }


                // 게임 시작하러 가니까 HUD 다시 켜줘
                if (UIManager.Instance != null) UIManager.Instance.SetHUDActive(true);
                
                // 지금 켜진 이 씬(Title)을 끄라고
                GameManager.Instance.currentStageName = SceneManager.GetActiveScene().name;

                // 좌표 설정 및 이동
                float x = DataManager.Instance.currentData.playerX;
                float y = DataManager.Instance.currentData.playerY;
                GameManager.Instance.NextSpawnPoint = new Vector2(x, y);

                GameManager.Instance.ChangeStage(DataManager.Instance.currentData.sceneName);
            }
        }
        else
        {
            Debug.Log("불러올 데이터가 없습니다.");
        }
    }

    public void ClickExit()
    {
        Application.Quit();
    }
}