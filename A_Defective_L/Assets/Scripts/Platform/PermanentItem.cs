using UnityEngine;

public class PermanentItem : MonoBehaviour
{
    [Header("설정")]
    // ★ 가장 중요: 맵에 배치된 아이템마다 이 이름을 다르게 적어야 함!
    // 예: Stage1_HP_01, Stage1_HP_02, Stage2_Kit_01 ...
    public string itemID; 
    
    public enum ItemType { MaxHealth, PotionCapacity }
    public ItemType type;
    public int amount = 1;
    public GameObject pickupEffect;

    private void Start()
    {
        // 1. 게임 시작 시, 이미 먹은 ID인지 장부 확인
        if (DataManager.Instance != null)
        {
            if (DataManager.Instance.CheckItemCollected(itemID))
            {
                // 이미 먹은 거면 조용히 사라짐
                gameObject.SetActive(false); 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // 2. 효과 적용
                if (type == ItemType.MaxHealth) stats.UpgradeMaxHealth(amount);
                else if (type == ItemType.PotionCapacity) stats.UpgradePotionCapacity();

                // 3. ★ 장부에 기록 (이 ID 먹었음!)
                if (DataManager.Instance != null)
                {
                    DataManager.Instance.RegisterItem(itemID);
                    DataManager.Instance.SaveDataToDisk(); // 즉시 저장
                }

                // 4. 이펙트 및 삭제
                if (pickupEffect != null) Instantiate(pickupEffect, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}