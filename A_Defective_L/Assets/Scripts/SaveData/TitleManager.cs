using UnityEngine;
using UnityEngine.SceneManagement; 

public class TitleManager : MonoBehaviour //타이틀 화면 관리
{
   private void Start()
    {
        // UI 씬 로드 (없을 경우에.)
        if (SceneManager.GetSceneByName("Scene_Gauge").isLoaded == false)
        {
             SceneManager.LoadScene("Scene_Gauge", LoadSceneMode.Additive);
        }

        // 타이틀이니까 체력바는 숨김
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

    // [새로 하기] 버튼용
    public void ClickNewGame()
    {
        DataManager.Instance.NewGame();

        // 2. ★ [핵심] 초기화된 데이터를 파일에도 저장!
        // 그래야 죽었을 때 로드하면 이 '초기 상태'가 불러와짐
        DataManager.Instance.SaveDataToDisk();

        if (GameManager.Instance != null)
        {
            // ★ 게임 시작하러 가니까 HUD 다시 켜줘!
           //if (UIManager.Instance != null) UIManager.Instance.SetHUDActive(true);

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
                // ★ [핵심 추가] 저장된 씬 이름 확인 (검문소)
                string savedScene = DataManager.Instance.currentData.sceneName;

                // "프롤로그", "컷신", "타이틀" 에서 저장된 데이터라면? -> 이어하기 금지!
                if (savedScene == "Prologue" || savedScene == "Cutscene_Intro" || savedScene == "Title")
                {
                    Debug.Log("⚠️ 프롤로그 데이터는 초기화.");

                    // 데이터 싹 비우고 (New Game 상태로)
                    DataManager.Instance.NewGame(); 
                    
                    // 1스테이지로 강제 이동
                    GameManager.Instance.currentStageName = "Title"; // 현재 위치는 타이틀
                    GameManager.Instance.ChangeStage("Prologue");      // 목표는 1스테이지
                    
                    // HUD 켜기
                    if (UIManager.Instance != null) UIManager.Instance.SetHUDActive(true);
                    return; // 함수 종료
                }


                /////

                // ★ 게임 시작하러 가니까 HUD 다시 켜줘!
                if (UIManager.Instance != null) UIManager.Instance.SetHUDActive(true);
                
                // ★ [강제 주입] "지금 켜진 이 씬(Title)을 끄라고!"
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