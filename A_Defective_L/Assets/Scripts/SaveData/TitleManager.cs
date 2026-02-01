using UnityEngine;
using UnityEngine.SceneManagement; // ★ 필수

public class TitleManager : MonoBehaviour
{
   private void Start()
    {
        // 1. UI 씬 로드 (없으면)
        if (SceneManager.GetSceneByName("Scene_Gauge").isLoaded == false)
        {
             SceneManager.LoadScene("Scene_Gauge", LoadSceneMode.Additive);
        }

        // 2. ★ 타이틀이니까 HUD(체력바 등)는 숨겨라!
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHUDActive(false); 
        }

        // 3. 매니저 설정
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