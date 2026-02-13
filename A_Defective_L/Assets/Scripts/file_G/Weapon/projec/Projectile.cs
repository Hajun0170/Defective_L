using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 15f; 
    public float lifeTime = 3f;

    private int damage;
    private Rigidbody2D rb;

    public GameObject hitEffectPrefab; // 이펙트 프리팹

    private AudioClip hitSound; //전달받은 사운드 저장용

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(int damageAmount, AudioClip sound = null, GameObject effectPrefab = null) 
    {
        this.damage = damageAmount;
        this.hitSound = sound; //소리 삽입

        // 전달받은 이펙트가 있으면 내 변수에 저장
        if (effectPrefab != null) 
        {
            this.hitEffectPrefab = effectPrefab;
        }

        if (rb != null)
        {

            rb.linearVelocity = transform.right * speed; 
        }

        Destroy(gameObject, lifeTime); // 시간 지나면 삭제
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        // 플레이어나, 다른 투사체 몬스터의 트리거는 무시하고 지나감

        if (collision.CompareTag("Player")) return;
        if (collision.isTrigger) return; // 트리거끼리는 충돌 무시 


        // 충돌 처리 로직 정리        
        // 벽이나 땅에 맞았을 때
        if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            DestroyProjectile();
        }
        
        // 일반 몬스터에 맞았을 때
        else if (collision.GetComponent<EnemyHealth>() != null)
        {
            collision.GetComponent<EnemyHealth>().TakeDamage(damage, transform);

            // 총알이 가지고 있던 타격음 재생
                if (hitSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(hitSound);
                }

            SpawnEffect();      // 이펙트 먼저 생성
            DestroyProjectile(); // 삭제
        }
        
        //보스에 맞았을 때
        else if (collision.GetComponent<BossController>() != null)
        {
            collision.GetComponent<BossController>().TakeDamage(damage);

            // 총알이 가지고 있던 타격음 재생
                if (hitSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(hitSound);
                }

            SpawnEffect();
            DestroyProjectile();
        }

        // 태그는 없지만 부딪힌 게 물체일 때 허공에 멈추지 말고 그냥 삭제
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