using UnityEngine;

public class CurrencyPickup : MonoBehaviour
{
    [Header("설정")]
    public int goldAmount = 1;   // 획득량
    public GameObject pickupEffect; 
    //사운드
    public AudioClip pickupSound;
    private void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 popForce = new Vector2(Random.Range(-2f, 2f), Random.Range(3f, 5f));
            rb.AddForce(popForce, ForceMode2D.Impulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // ★ [수정] 직접 DataManager에 넣지 말고, 플레이어에게 전달!
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            
            if (stats != null)
            {
                // "플레이어야, 여기 돈 주웠어. 네가 처리해."
                stats.AddGold(goldAmount); 
                
                // 이펙트 생성
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }

                // ★ [추가] 획득 사운드 재생
                // (오브젝트가 사라지더라도 소리는 남아야 하므로 AudioManager나 PlayClipAtPoint 사용)
                if (AudioManager.Instance != null && pickupSound != null)
                {
                    AudioManager.Instance.PlaySFX(pickupSound);
                }
             
                // 삭제
                Destroy(gameObject);
            }
        }
    }
}