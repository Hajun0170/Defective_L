using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Item Settings")]
    public Weapon weaponData; // ★ 인스펙터에서 떨어트릴 무기 데이터(ScriptableObject) 연결
    public GameObject pickupEffect; // (선택) 획득 이펙트

    // ★ [추가] 이펙트 오프셋 설정 (X, Y, Z 조정 가능)
    // 기본값을 (0, 0, 0)으로 설정하여 별도 설정이 없으면 기존 위치에 생성되게 함
    public Vector3 effectOffset = Vector3.zero;

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
                if (pickupEffect != null)
                {
                    // ★ [수정] 현재 아이템 위치에 설정한 오프셋을 더해서 최종 생성 위치 결정
                    Vector3 spawnPos = transform.position + effectOffset;
                    Instantiate(pickupEffect, spawnPos, Quaternion.identity);
                }
                
                // 아이템 삭제
                Destroy(gameObject);
            }
        }
    }
}