using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 3f;
    private int damage;

    public void Initialize(int dmg)
    {
        this.damage = dmg;
        Destroy(gameObject, lifeTime); // 일정 시간 후 삭제
        
        // 이동 방향 설정 (생성 시 회전값 기준 앞으로)
        // Unity 6: linearVelocity 사용
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if(rb != null)
        {
            rb.linearVelocity = transform.right * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            Destroy(gameObject); // 적중 시 삭제
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject); // 벽 충돌 시 삭제
        }
    }
}