using System.Collections;
using UnityEngine;

public class DamageZone_Trap : MonoBehaviour //필드 함정에서 사용. 낙하구간 함정에 빠지면 지정한 위치로 이동
{
    [Header("Settings")]
    [Tooltip("함정에 닿았을 때 플레이어가 이동할 위치. 빈 오브젝트")]
    [SerializeField] private Transform respawnPoint; 
    [SerializeField] private float respawnDelay = 0.5f; // 이동 전 대기 시간 (연출용)

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
        MonoBehaviour playerScript = player.GetComponent<MonoBehaviour>(); // PlayerController

        //플레이어 조작 중단 (이동 중에 추가 이동 방지)
        if (rb != null) 
        {
            rb.linearVelocity = Vector2.zero; // 가속도 초기화
            rb.simulated = false; // 물리 연산 잠시 중지 (추락 방지)
        }

        // 대기. 암전되는 시간
        yield return new WaitForSeconds(respawnDelay);

        // 플레이어 위치 강제 이동
        player.transform.position = respawnPoint.position;

        // 플레이어 조작 해제
        if (rb != null)
        {
            rb.simulated = true;
        }
    }
}