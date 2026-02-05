using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Item Settings")]
    public Weapon weaponData; // ★ 인스펙터에서 떨어트릴 무기 데이터(ScriptableObject) 연결
    public GameObject pickupEffect; // (선택) 획득 이펙트

    [Header("Floating Animation")]
    public float floatSpeed = 2f;
    public float floatHeight = 0.2f;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
        
        // 아이콘 표시 (스프라이트 렌더러가 있다면)
        if (weaponData != null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sprite = weaponData.icon;
        }
    }

    private void Update()
    {
        // 둥둥 떠있는 연출
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 플레이어의 WeaponManager 찾기
            WeaponManager weaponManager = collision.GetComponent<WeaponManager>();
            
            if (weaponManager != null && weaponData != null)
            {
                // ★ 여기서 AddWeapon을 부르면 -> 리스트 추가 & 즉시 장착 & 저장까지 자동으로 됨
                weaponManager.AddWeapon(weaponData);
                
                // 효과음 및 이펙트
                if (pickupEffect != null) Instantiate(pickupEffect, transform.position, Quaternion.identity);
                
                // 아이템 삭제
                Destroy(gameObject);
            }
        }
    }
}