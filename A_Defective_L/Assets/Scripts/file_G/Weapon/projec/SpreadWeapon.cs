using UnityEngine;
// --- 확산 원거리 (투척 나이프) ---
public class SpreadWeapon : RangedWeapon
{
    [SerializeField] private int projectileCount = 3;
    [SerializeField] private float spreadAngle = 15f;

    protected override void SpawnProjectile(Transform firePoint, PlayerStats stats)
    {
        int totalDamage = GetFinalDamage(stats);
        int damagePerProjectile = totalDamage / projectileCount; // 데미지 분산

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = spreadAngle * (i - (projectileCount - 1) / 2f);
            Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, angle);

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, rot);
            proj.GetComponent<Projectile>().Initialize(damagePerProjectile);
        }
    }
}