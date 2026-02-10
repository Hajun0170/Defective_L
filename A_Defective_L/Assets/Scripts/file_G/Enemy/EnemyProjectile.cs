using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 10f;
    public int damage = 1;
    public float lifeTime = 5f; // 너무 오래 날아가면 삭제
    public GameObject hitEffect; // 폭발 이펙트

    [Header("Audio")] // ★ 소리 추가
    public AudioClip spawnSound; // 인스펙터에 '뿅' 하는 소리 파일 넣기
    public float soundVolume = 0.5f;

    private void Start()
    {   
        // 1. 소리 재생 (AudioSource 컴포넌트 없이도 1회성 재생 가능)
        if (spawnSound != null)
        {
            AudioSource.PlayClipAtPoint(spawnSound, transform.position, soundVolume);
        }
        
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
                // ★ [핵심 수정] 플레이어가 무적 상태(회피 중)라면?
                // 데미지도 주지 않고, 투사체도 파괴하지 않고, 여기서 코드 종료(return)!
                // 결과적으로 투사체는 그냥 지나가게 됨
                if (player.IsInvincible) return;
                
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