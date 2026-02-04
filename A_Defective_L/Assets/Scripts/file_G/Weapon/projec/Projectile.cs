using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 15f; 
    public float lifeTime = 3f;

    private int damage;
    private Rigidbody2D rb;

    public GameObject hitEffectPrefab; // 이펙트 프리팹

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(int damageAmount) 
    {
        this.damage = damageAmount;

        if (rb != null)
        {
            // Unity 6버전은 linearVelocity, 구버전은 velocity
            // (혹시 에러나면 velocity로 바꾸세요)  
            rb.linearVelocity = transform.right * speed; 
        }

        Destroy(gameObject, lifeTime); // 시간 지나면 삭제
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ====================================================
        // ★ [핵심 1] 나 자신(플레이어)이나, 다른 투사체, 
        // 혹은 몬스터의 '감지 범위(Trigger)'는 무시하고 지나가야 함!
        // ====================================================
        if (collision.CompareTag("Player")) return;
        if (collision.isTrigger) return; // 트리거끼리는 충돌 무시 (중요!)


        // ====================================================
        // ★ [핵심 2] 충돌 처리 로직 정리 (순서 중요)
        // ====================================================
        
        // 1. 벽이나 땅에 맞았을 때
        if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            DestroyProjectile();
        }
        
        // 2. 일반 몬스터에 맞았을 때
        else if (collision.GetComponent<EnemyHealth>() != null)
        {
            collision.GetComponent<EnemyHealth>().TakeDamage(damage, transform);
            SpawnEffect();      // 이펙트 먼저 생성
            DestroyProjectile(); // 그 다음 삭제
        }
        
        // 3. 보스에 맞았을 때
        else if (collision.GetComponent<BossController>() != null)
        {
            collision.GetComponent<BossController>().TakeDamage(damage);
            SpawnEffect();
            DestroyProjectile();
        }

        // 4. (예외 처리) 태그는 없지만 부딪힌 게 '물체(Collider)'라면?
        // 허공에 멈추지 말고 그냥 삭제해라.
        else 
        {
             DestroyProjectile();
        }
    }

    // 코드 중복을 줄이기 위한 함수들
    void SpawnEffect()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}