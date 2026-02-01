using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("설정")]
    public string transferSceneName; // 이동할 씬 이름 (예: Stage_2)
    public int targetSpawnID;        // 그 씬의 몇 번 위치로 갈 건지

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. 데이터 저장 (DataManager에 다음 위치 전달)
            if (DataManager.Instance != null)
                DataManager.Instance.nextSpawnPointID = targetSpawnID;

            // 2. 플레이어 스탯도 임시 저장 (안전장치)
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            if (stats != null && GameManager.Instance != null)
            {
                GameManager.Instance.SaveCurrentStatus(
                    stats.CurrentHealth, 
                    stats.CurrentGauge, 
                    stats.CurrentTickets
                );
            }

            // 3. ★ GameManager에게 "맵만 바꿔줘!" 요청
            if (GameManager.Instance != null)
            {
                // 페이드 아웃 효과가 필요하면 여기서 UI 매니저 호출 가능
                // UIManager.Instance.FadeOut(); 
                
                GameManager.Instance.ChangeStage(transferSceneName);
            }
            else
            {
                // 매니저 없으면 그냥 이동 (테스트용)
                UnityEngine.SceneManagement.SceneManager.LoadScene(transferSceneName);
            }
/*
        // 1. 데이터 매니저 확인
            if (DataManager.Instance != null)
                DataManager.Instance.nextSpawnPointID = targetSpawnID;

            // 2. 씬 매니저 확인 (★ 여기가 핵심)
            if (SceneTransitionManager.Instance != null)
            {
                // 매니저가 있으면 우아하게 페이드 아웃
                SceneTransitionManager.Instance.LoadScene(transferSceneName);
            }
      
            */
        }
    }
}