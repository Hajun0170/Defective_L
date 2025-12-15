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
        else
        {
            Debug.Log("게이지 부족!");
        }
    }

    // 자식이 오버라이드할 수 있도록 virtual 선언
    protected virtual void SpawnProjectile(Transform firePoint, PlayerStats stats)
    {
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        // 생성된 투사체에 데미지 주입
        proj.GetComponent<Projectile>().Initialize(GetFinalDamage(stats));
    }
}