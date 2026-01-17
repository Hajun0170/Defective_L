using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 15f; 
    public float lifeTime = 3f;

    private int damage;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // ★ [핵심 수정] 매개변수에서 'Vector2 direction'을 지웠습니다!
    // 이제 데미지 하나만 받습니다.
    
    public void Initialize(int damageAmount) 
    {
        this.damage = damageAmount;

        // 방향은 입력받는 게 아니라, 
        // "이미 회전된 내 몸의 앞쪽(transform.right)"으로 날아가게 합니다.
        if (rb != null)
        {
            // Unity 6버전은 linearVelocity, 구버전(2022이하)은 velocity
            rb.linearVelocity = transform.right * speed; 
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌 로직 (태그 확인 등)
        if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
        else if (collision.GetComponent<EnemyHealth>() != null)
        {
            collision.GetComponent<EnemyHealth>().TakeDamage(damage, transform);
            Destroy(gameObject);
        }
    }
}