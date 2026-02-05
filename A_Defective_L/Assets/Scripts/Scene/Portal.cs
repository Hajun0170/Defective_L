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

           // ★ [핵심 수정] 플레이어의 모든 스탯(최대체력, 돈 포함)을 여기서 강제로 저장!
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // GameManager를 거치지 않고 직접 저장 함수 호출 (가장 안전함)
                stats.SaveStatsToManager(); 
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
        }
    }
}