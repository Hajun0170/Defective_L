using UnityEngine;

public class EnemyProjectile : MonoBehaviour //적의 투사체를 날리는 코드, 대부분의 적들이 이걸 이용하여 발사.
{
    [Header("Settings")]
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 5f; // 너무 오래 날아가면 삭제
    public GameObject hitEffect; // 폭발 이펙트

    [Header("Audio")]
    public AudioClip spawnSound;
    public float soundVolume = 0.5f;

   private void Start()
{   
    // AudioManager를 통해 재생 (SFX 그룹으로 자동 연결)
    if (spawnSound != null && AudioManager.Instance != null)
    {
        AudioManager.Instance.PlaySFX(spawnSound);
    }
    
    Destroy(gameObject, lifeTime);
}

    private void Update()
    {
        // 앞으로 전진 (발사될 때 회전값 기준)
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어와 충돌 시
        if (collision.CompareTag("Player"))
        {
            PlayerStats player = collision.GetComponent<PlayerStats>();
            if (player != null)
            {
                // 플레이어가 무적 상태(회피 중)일 경우
                // 데미지 x, 투사체도 파괴하지 않고, 여기서 코드 종료(return)함  투사체는 그냥 지나
                if (player.IsInvincible) return;
                
                // 플레이어에게 데미지 입힘 (공격자 transform 넘겨줌)
                player.TakeDamage(damage, transform);
            }
            HitAndDestroy();
        }
        // 벽이나 땅에 충돌 시 (태그 확인)
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