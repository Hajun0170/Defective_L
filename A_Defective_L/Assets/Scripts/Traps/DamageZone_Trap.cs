using System.Collections;
using UnityEngine;

public class DamageZone_Trap : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("함정에 닿았을 때 플레이어가 이동할 안전한 위치 (빈 오브젝트)")]
    [SerializeField] private Transform respawnPoint; 
    [SerializeField] private float respawnDelay = 0.5f; // 이동 전 대기 시간 (암전 연출용)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // OnTriggerStay 대신 Enter를 씁니다. (닿는 순간 한 번만 실행하기 위해)
        if (collision.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            
            // 1. 데미지 주기
            if (playerStats != null)
            {
                playerStats.TakeDamage(1, transform);
            }

            // 2. 리스폰 지점이 설정되어 있다면 그쪽으로 이동 로직 실행
            if (respawnPoint != null)
            {
                StartCoroutine(RespawnRoutine(collision));
            }
        }
    }

    private IEnumerator RespawnRoutine(Collider2D player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        MonoBehaviour playerScript = player.GetComponent<MonoBehaviour>(); // 플레이어 조작 스크립트 (예: PlayerController)

        // A. 플레이어 조작 얼리기 (중요: 이동 중에 또 움직이면 안 됨)
        if (rb != null) 
        {
            rb.linearVelocity = Vector2.zero; // 가속도 초기화
            rb.simulated = false; // 물리 연산 잠시 끄기 (추락 방지)
        }
        
        // ★ 여기서 화면 암전(Fade Out) 함수를 호출하면 좋습니다.
        // ex) UIManager.Instance.FadeOut();

        // B. 대기 (암전되는 시간 벌기)
        yield return new WaitForSeconds(respawnDelay);

        // C. 플레이어 위치 강제 이동
        player.transform.position = respawnPoint.position;

        // ★ 여기서 화면 밝아짐(Fade In) 함수를 호출하면 좋습니다.
        // ex) UIManager.Instance.FadeIn();

        // D. 플레이어 조작 풀기
        if (rb != null)
        {
            rb.simulated = true;
        }
    }
}