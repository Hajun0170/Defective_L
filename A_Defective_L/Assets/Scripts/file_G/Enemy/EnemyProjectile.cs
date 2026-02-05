using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 5f; // 너무 오래 날아가면 삭제
    public GameObject hitEffect; // 폭발 이펙트

    private void Start()
    {
        // 일정 시간 뒤 자동 삭제 (메모리 관리)
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // 앞으로 전진 (발사될 때 회전값 기준)
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 플레이어와 충돌 시
        if (collision.CompareTag("Player"))
        {
            PlayerStats player = collision.GetComponent<PlayerStats>();
            if (player != null)
            {
                // 플레이어에게 데미지 입힘 (공격자 transform 넘겨줌)
                player.TakeDamage(damage, transform);
            }
            HitAndDestroy();
        }
        // 2. 벽이나 땅에 충돌 시 (레이어 확인 or 태그 확인)
        else if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            HitAndDestroy();
        }
    }

    void HitAndDestroy()
    {
        // 이펙트 생성
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        // 투사체 삭제
        Destroy(gameObject);
    }
}