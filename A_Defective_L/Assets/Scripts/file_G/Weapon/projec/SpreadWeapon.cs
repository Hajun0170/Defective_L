using UnityEngine;
// --- 확산 원거리 (투척 나이프) ---
public class SpreadWeapon : RangedWeapon
{
    [SerializeField] private int projectileCount = 3;
    [SerializeField] private float spreadAngle = 15f;

    protected override void SpawnProjectile(Transform firePoint, PlayerStats stats)
    {
        int totalDamage = GetFinalDamage(stats);
        int damagePerProjectile = totalDamage / projectileCount;

        // [수정된 부분] 캐릭터가 보고 있는 방향(각도) 계산
        bool isFlipped = firePoint.lossyScale.x < 0;
        float baseAngle = isFlipped ? 180f : 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            // 부채꼴 각도 계산
            float angleStep = spreadAngle * (i - (projectileCount - 1) / 2f);
            
            // 기준 각도(baseAngle)에 부채꼴 각도(angleStep)를 더함
            // 주의: 왼쪽을 볼 때는 부채꼴 각도도 반대로 적용해야 자연스러울 수 있으므로 -부호를 고려할 수 있으나,
            // 여기서는 단순하게 기준 각도에 더하는 방식으로 처리합니다.
            Quaternion rot = Quaternion.Euler(0, 0, baseAngle + angleStep);

            GameObject proj = Instantiate(projectilePrefab, firePoint.position, rot);
            proj.GetComponent<Projectile>().Initialize(damagePerProjectile);
        }
    }



/*
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
    */
}