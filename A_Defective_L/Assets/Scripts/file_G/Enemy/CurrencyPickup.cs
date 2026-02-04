using UnityEngine;

public class CurrencyPickup : MonoBehaviour
{
    [Header("설정")]
    public int goldAmount = 1;   // 획득량
    public GameObject pickupEffect; // 획득 시 반짝이는 이펙트

    // 아이템이 생성될 때 살짝 튀어오르는 효과
    private void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 위쪽 랜덤한 방향으로 톡 튀어오름
            Vector2 popForce = new Vector2(Random.Range(-2f, 2f), Random.Range(3f, 5f));
            rb.AddForce(popForce, ForceMode2D.Impulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어 몸체(태그 Player)와 닿았을 때
        if (collision.CompareTag("Player"))
        {
            // 1. 데이터 매니저에 돈 추가
            if (DataManager.Instance != null)
            {
                DataManager.Instance.currentData.gold += goldAmount;
                Debug.Log($"💰 골드 획득! 현재: {DataManager.Instance.currentData.gold}");
                
                // (선택) 즉시 저장하려면 아래 주석 해제 (보통은 쉼터에서 저장)
                // DataManager.Instance.SaveDataToDisk();
            }

            // 2. 획득 이펙트 생성
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // 3. 아이템 삭제
            Destroy(gameObject);
        }
    }
}