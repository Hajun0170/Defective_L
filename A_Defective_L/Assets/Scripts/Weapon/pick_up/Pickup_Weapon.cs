using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Pickup_Weapon : MonoBehaviour
{
    [Header("Config")]
    public WeaponId eWeaponId;     // EnemyDropper가 세팅
    public int iMatOnRepeat = 1;   // EnemyDropper가 세팅
    public bool bDestroyOnPickup = true;

    void Reset(){
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        gameObject.tag = "Pickup";
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var arm  = other.GetComponentInParent<ArmamentController>();
        var arch = other.GetComponentInParent<Archive>();
        if (arm == null || arch == null) return;

        if (arch.Has(eWeaponId))
        {
            // 이미 보유 → 재료 지급
            ArchiveCompatUtil.AddMaterial(arch, eWeaponId, iMatOnRepeat);
        }
        else
        {
            // 무기 장착
            var prefab = WeaponRegistry.Instance.GetPrefab(eWeaponId);
            if (prefab != null)
            {
                arm.Equip(prefab);
                // 소유 상태 기록 (SetHas/Unlock/AddWeapon 중 가능한 걸 자동 호출)
                ArchiveCompatUtil.MarkOwned(arch, eWeaponId);
            }
            else
            {
                Debug.LogWarning($"[Pickup_Weapon] No prefab for {eWeaponId} in WeaponRegistry.");
            }
        }

        if (bDestroyOnPickup) Destroy(gameObject);
    }
}
