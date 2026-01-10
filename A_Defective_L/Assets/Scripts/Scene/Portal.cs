using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Settings")]
    public string sceneName; 
    public Vector2 spawnPoint; 

    private bool isTriggered = false; // 중복 진입 방지

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 이미 작동했거나 플레이어가 아니면 무시
        if (isTriggered || !collision.CompareTag("Player")) return;

        isTriggered = true; // 문 잠그기

        PlayerStats stats = collision.GetComponent<PlayerStats>();
        
        // 데이터 저장
        if (GameManager.Instance != null && stats != null)
        {
            GameManager.Instance.SaveCurrentStatus(
                stats.CurrentHealth, 
                stats.CurrentGauge, 
                stats.CurrentTickets
            );
            
            // 다음 위치 저장
            GameManager.Instance.NextSpawnPoint = spawnPoint;
        }

        // [팁] 로딩되는 동안 플레이어가 움직이거나 공격하지 못하게 막기
        // collision.GetComponent<PlayerMovement>().enabled = false; 

        // 씬 로드
        SceneManager.LoadScene(sceneName);
    }
}