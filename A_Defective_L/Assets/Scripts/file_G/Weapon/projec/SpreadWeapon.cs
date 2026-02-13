using UnityEngine;

[CreateAssetMenu(fileName = "New Spread", menuName = "Weapon/Spread Weapon")]
public class SpreadWeapon : RangedWeapon //투척 칼날 = 흩뿌리는 무기
{
    [Header("Spread Settings")]
    [SerializeField] private int projectileCount = 3; 
    [SerializeField] private float spreadAngle = 15f; 

    protected override void SpawnProjectile(Transform firePoint, PlayerStats stats)
    {
        int totalDamage = GetFinalDamage(stats);
        int damagePerProjectile = Mathf.Max(1, totalDamage / projectileCount);

        // 캐릭터 방향 확인
        bool isFlipped = firePoint.lossyScale.x < 0;
        float baseAngle = isFlipped ? 180f : 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            // 각도 계산
            float angleOffset = spreadAngle * (i - (projectileCount - 1) / 2f);
            float currentAngle = baseAngle + angleOffset;
            
            // 각도를 다 돌려놓음
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);

            // 생성 및 초기화
            if (projectilePrefab != null)
            {
                // 생성할 때 회전을 적용해서 만듬
                GameObject proj = Instantiate(projectilePrefab, firePoint.position, rotation);


                // 소리(hitSound)와 이펙트(hitEffectPrefab)를 같이 넘김
                // (hitSound와 hitEffectPrefab: Weapon 클래스에 존재함)
                Projectile p = proj.GetComponent<Projectile>();
                if (p != null)
                {
                    p.Initialize(damagePerProjectile, hitSound, hitEffectPrefab);
                }
            }
        }
    }
}