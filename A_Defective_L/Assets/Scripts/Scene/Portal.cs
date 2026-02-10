using UnityEngine;

public class Portal : MonoBehaviour // 씬 이동 코드. 닿을 경우 Spawn 포인트에 맞춰짐
{
    [Header("설정")]
    public string transferSceneName; // 이동할 씬 이름 예) Stage_1
    public int targetSpawnID;        // 그 씬의 어떤 위치로 갈 건지 설정할 ID

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 데이터 저장: DataManager에 다음 위치 전달
            if (DataManager.Instance != null)
                DataManager.Instance.nextSpawnPointID = targetSpawnID;

           //플레이어의 스탯(최대체력, 돈, 키트)을 저장
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // GameManager를 거치지 않고 직접 저장 함수 호출 
                stats.SaveStatsToManager(); 
            }

            //  GameManager에게 맵만 바꾸는 방식 사용 (멀티씬용)
            if (GameManager.Instance != null)
            {            
                GameManager.Instance.ChangeStage(transferSceneName);
            }
            else
            {
                // 매니저 없으면 그냥 이동 (테스트 버전이라 실제로는 사용 안 함)
                UnityEngine.SceneManagement.SceneManager.LoadScene(transferSceneName);
            }
        }
    }
}