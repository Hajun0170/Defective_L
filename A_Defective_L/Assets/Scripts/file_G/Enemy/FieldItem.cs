using UnityEngine;

public class FieldItem : MonoBehaviour
{
    public enum ItemType { MaxHealthUp, PotionCapacityUp, JustPotionRefill }
    
    [Header("설정")]
    public ItemType itemType;
    public string itemID; // ★ [중요] 아이템마다 다르게 적어줘야 함! (예: Map1_HP_01)
    public int amount = 1; // 체력 증가량 등
    public GameObject pickupEffect;

    void Start()
    {
        /*
        // 이미 먹은 아이템인지 확인하고, 먹었다면 삭제
        if (DataManager.Instance != null)
        {
            if (DataManager.Instance.currentData.collectedItems.Contains(itemID))
            {
                gameObject.SetActive(false); // 이미 먹음
            }
        }
        */
        // ★ [핵심] 태어나자마자 장부 확인
        // 만약 이미 먹은 아이템 목록에 내 ID가 있다면? -> 바로 자폭(삭제)
        if (DataManager.Instance != null && DataManager.Instance.CheckItemCollected(itemID))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            if (stats != null)
            {
                ApplyEffect(stats);
                
                // 획득 기록 저장 (JustPotionRefill 같은 소모품은 기록 안 함)
               /* if (itemType != ItemType.JustPotionRefill)
                {
                    DataManager.Instance.currentData.collectedItems.Add(itemID);
                }
                */

                // ★ [핵심] 먹었음을 매니저에 신고
                if (DataManager.Instance != null)
                {
                    DataManager.Instance.RegisterItem(itemID);
                }

                // 이펙트 및 삭제
                if (pickupEffect != null) Instantiate(pickupEffect, transform.position, Quaternion.identity);
                // 삭제
                Destroy(gameObject);
                // gameObject.SetActive(false); // 삭제
            }
        }
    }

    void ApplyEffect(PlayerStats stats)
    {
        switch (itemType)
        {
            case ItemType.MaxHealthUp:
                stats.UpgradeMaxHealth(amount); // 예: 2 (하트 1칸)
                break;
                
            case ItemType.PotionCapacityUp:
                stats.UpgradePotionCapacity();
                break;

            case ItemType.JustPotionRefill:
                // 그냥 키트 하나 충전 (필드 드랍용)
                DataManager.Instance.currentData.currentPotions++;
                if(DataManager.Instance.currentData.currentPotions > DataManager.Instance.currentData.potionCapacity)
                   DataManager.Instance.currentData.currentPotions = DataManager.Instance.currentData.potionCapacity;
                
                // UI 갱신 필요하면 stats.UpdateAllUI() 호출 (public으로 바꿔야 함)
                break;
        }
    }
}