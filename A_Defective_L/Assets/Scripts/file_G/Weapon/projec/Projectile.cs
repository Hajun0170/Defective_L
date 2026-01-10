using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 10f;
    public float lifeTime = 3f; // 3초 뒤 자동 삭제

    private int damage;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 날아가야 하니 중력 0으로 설정 권장
        if(rb != null) rb.gravityScale = 0; 
    }

    // Weapon이 생성하자마자 호출해주는 초기화 함수
    public void Initialize(int damageAmount, Vector2 direction)
    {
        this.damage = damageAmount;
        
        // 방향으로 속도 주기
        if (rb != null)
        {
            rb.linearVelocity = direction * speed; // Unity 6 (구버전은 velocity)
        }
        
        // 일정 시간 뒤 삭제 (메모리 관리)
        Destroy(gameObject, lifeTime);
    }

    // 적과 부딪혔을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 적을 찾아서 데미지 주기
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            
            // (선택) 타격 이펙트 생성
            // Instantiate(hitEffect, transform.position, Quaternion.identity);
            
            // 나 자신(총알)은 삭제
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            // 벽에 맞아도 삭제
            Destroy(gameObject);
        }
    }
}