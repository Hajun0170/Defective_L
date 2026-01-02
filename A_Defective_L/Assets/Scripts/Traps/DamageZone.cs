using UnityEngine;
public class DamageZone : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        // 플레이어가 닿으면 데미지 1 줌
        if (collision.CompareTag("Player"))
        {
            // 넉백 방향 계산 (플레이어가 가시보다 왼쪽이면 왼쪽으로 튕김)
            // Vector2 pushDir = (collision.transform.position - transform.position).normalized;

            collision.GetComponent<PlayerStats>()?.TakeDamage(1, transform);
        }
    }
}