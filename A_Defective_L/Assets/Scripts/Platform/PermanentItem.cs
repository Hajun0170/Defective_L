using UnityEngine;

public class PermanentItem : MonoBehaviour //아이템: 회복킷, 최대체력 아이템.
{
    [Header("설정")]

    public string itemID; 
    
    public enum ItemType { MaxHealth, PotionCapacity }
    public ItemType type;
    public int amount = 1;
    public GameObject pickupEffect;

    [Header("Sound")]
    public AudioClip pickupSound;

    private void Start()
    {
        //게임 시작 시, 이미 획득한 ID를 가진 아이템인지 확인
        if (DataManager.Instance != null)
        {
            if (DataManager.Instance.CheckItemCollected(itemID))
            {
                //이미 먹은 거면 아이템이면 사라짐
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
                //효과 적용
                if (type == ItemType.MaxHealth) stats.UpgradeMaxHealth(amount);
                else if (type == ItemType.PotionCapacity) stats.UpgradePotionCapacity();

                // 아이템 목록(데이터 매니저)에 기록
                if (DataManager.Instance != null)
                {
                    DataManager.Instance.RegisterItem(itemID);
                    DataManager.Instance.SaveDataToDisk(); // 즉시 저장
                }

                // 아이템이 Destroy, 소리는 않 끊기게 AudioManager 사용
                if (AudioManager.Instance != null && pickupSound != null)
                {
                    AudioManager.Instance.PlaySFX(pickupSound);
                }

                // 이펙트 및 삭제
                if (pickupEffect != null) Instantiate(pickupEffect, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}