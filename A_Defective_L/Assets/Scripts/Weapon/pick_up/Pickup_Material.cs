using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Pickup_Material : MonoBehaviour
{
    [Header("Config")]
    public WeaponId eWeaponId;
    public int iAmount = 1;
    public bool bDestroyOnPickup = true;

    void Reset(){
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        gameObject.tag = "Pickup";
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var arch = other.GetComponentInParent<Archive>();
        if (arch == null) return;

        // ★ bool 반환값 기대하지 말고, 호환 유틸로 호출
        ArchiveCompatUtil.AddMaterial(arch, eWeaponId, iAmount);

        if (bDestroyOnPickup) Destroy(gameObject);
    }
}
