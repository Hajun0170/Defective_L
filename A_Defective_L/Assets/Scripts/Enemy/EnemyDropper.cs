using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyDropper : MonoBehaviour
{
    [Header("Drop Config")]
    public WeaponId eWeaponId = WeaponId.Sword;
    public GameObject pWeaponPickupPrefab;   // Pickup_Weapon가 붙은 프리팹
    public GameObject pMaterialPickupPrefab; // Pickup_Material가 붙은 프리팹
    public int iMaterialOnRepeat = 1;
    public float fDropChance = 1f;           // 1=100%, 0.5=50%

    bool bDropped; // 중복 드랍 방지

    void Awake(){
        GetComponent<EnemyHealth>().OnDeath += OnEnemyDeath;
    }

    void OnEnemyDeath(){
        if (bDropped) return;
        bDropped = true;

        if (Random.value > fDropChance) return; // 확률 미적중 시 드랍 없음

        // 플레이어 Archive 상태에 따라 '무기' 또는 '재료' 드랍
        var player = GameObject.FindGameObjectWithTag("Player");
        var archive = player? player.GetComponent<Archive>() : null;

        if (archive!=null && !archive.Has(eWeaponId)){
            // 무기 픽업 드랍(첫 격파)
            if (pWeaponPickupPrefab){
                var go = Instantiate(pWeaponPickupPrefab, transform.position, Quaternion.identity);
                var comp = go.GetComponent<Pickup_Weapon>();
                if (comp) { comp.eWeaponId = eWeaponId; comp.iMatOnRepeat = iMaterialOnRepeat; }
            }
        }else{
            // 이미 보유 → 재료 드랍
            if (pMaterialPickupPrefab){
                var go = Instantiate(pMaterialPickupPrefab, transform.position, Quaternion.identity);
                var comp = go.GetComponent<Pickup_Material>();
                if (comp) { comp.eWeaponId = eWeaponId; comp.iAmount = iMaterialOnRepeat; }
            }
        }
    }
}
