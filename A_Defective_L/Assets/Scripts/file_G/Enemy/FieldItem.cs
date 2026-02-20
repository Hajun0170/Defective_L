using UnityEngine;

public class FieldItem : MonoBehaviour //게임 내 입수하는 아이템(최대 체력과 회복 키트. 습득 요소. 중복 방지 기능 포함)
{
    public enum ItemType { MaxHealthUp, PotionCapacityUp, JustPotionRefill }
    
    [Header("설정")]
    public ItemType itemType;
    public string itemID; // 아이템마다 다르게 적어줌 (HP_01)
    public int amount = 1; // 체력 증가량 등
    public GameObject pickupEffect;

    void Start()
    {
       
        // 태어나자마자 목록 확인
        // 만약 이미 먹은 아이템 목록에 내 ID가 있다면 바로 삭제
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

                // 획득을 매니저에 신고
                if (DataManager.Instance != null)
                {
                    DataManager.Instance.RegisterItem(itemID);
                }

                // 이펙트 및 삭제
                if (pickupEffect != null) Instantiate(pickupEffect, transform.position, Quaternion.identity);
                // 삭제
                Destroy(gameObject);
            }
        }
    }

    void ApplyEffect(PlayerStats stats)
    {
        switch (itemType)
        {
            case ItemType.MaxHealthUp:
                stats.UpgradeMaxHealth(amount); // 하트 1칸
                break;
                
            case ItemType.PotionCapacityUp:
                stats.UpgradePotionCapacity();
                break;

            case ItemType.JustPotionRefill:
                // 그냥 키트 1개 충전 (필드 드랍용)
                DataManager.Instance.currentData.currentPotions++;
                if(DataManager.Instance.currentData.currentPotions > DataManager.Instance.currentData.potionCapacity)
                   DataManager.Instance.currentData.currentPotions = DataManager.Instance.currentData.potionCapacity;
                
                break;
        }
    }
}