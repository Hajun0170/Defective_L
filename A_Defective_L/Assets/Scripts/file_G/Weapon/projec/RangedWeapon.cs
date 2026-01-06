using UnityEngine;

// --- 일반 원거리 (로켓 펀치) ---
public class RangedWeapon : Weapon
{
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected int gaugeCost = 5; // 소모량 5
  
    public override void PerformAttack(Transform firePoint, PlayerStats playerStats)
    {
        if (Time.time < nextAttackTime) return;

        if (playerStats.UseGauge(gaugeCost))
        {
            SpawnProjectile(firePoint, playerStats);
            nextAttackTime = Time.time + attackRate;
        }
    }

    // 자식이 오버라이드할 수 있도록 virtual 선언
    protected virtual void SpawnProjectile(Transform firePoint, PlayerStats stats)
    {
        // 1. 발사 위치(FirePoint)의 월드 스케일이 음수인지 확인 (캐릭터가 뒤집혔는지)
        // FirePoint는 Player의 자식이므로, Player가 뒤집히면 lossyScale.x도 음수가 됩니다.
        bool isFlipped = firePoint.lossyScale.x < 0;

        // 2. 뒤집혔다면 180도 회전(왼쪽), 아니면 0도(오른쪽)
        Quaternion rotation = isFlipped ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
        
        // 3. 투사체 생성   
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, rotation);
        // 생성된 투사체에 데미지 주입
        proj.GetComponent<Projectile>().Initialize(GetFinalDamage(stats));
    }
}