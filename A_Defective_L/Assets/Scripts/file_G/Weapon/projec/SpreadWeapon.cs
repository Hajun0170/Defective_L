using UnityEngine;

[CreateAssetMenu(fileName = "New Spread", menuName = "Weapon/Spread Weapon")]
public class SpreadWeapon : RangedWeapon
{
    [Header("Spread Settings")]
    [SerializeField] private int projectileCount = 3; 
    [SerializeField] private float spreadAngle = 15f; 

    protected override void SpawnProjectile(Transform firePoint, PlayerStats stats)
    {
        int totalDamage = GetFinalDamage(stats);
        int damagePerProjectile = Mathf.Max(1, totalDamage / projectileCount);

        // 1. 캐릭터 방향 확인
        bool isFlipped = firePoint.lossyScale.x < 0;
        float baseAngle = isFlipped ? 180f : 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            // 2. 각도 계산
            float angleOffset = spreadAngle * (i - (projectileCount - 1) / 2f);
            float currentAngle = baseAngle + angleOffset;
            
            // ★ 이미 여기서 각도를 다 돌려놨습니다.
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);

            // 3. 생성 및 초기화
            if (projectilePrefab != null)
            {
                // 생성할 때 이미 회전(rotation)을 적용해서 만듭니다.
                GameObject proj = Instantiate(projectilePrefab, firePoint.position, rotation);

                // ★ [핵심 수정] 
                // 이제 방향(Vector2)을 계산해서 넣어줄 필요가 없습니다.
                // 투사체 스스로 "내 앞쪽(transform.right)으로 날아가라"고 되어 있으니까요.
                // 데미지만 쏙 넣어주세요!
                //proj.GetComponent<Projectile>()?.Initialize(damagePerProjectile);
                // ★ [수정 후] 소리(hitSound)와 이펙트(hitEffectPrefab)도 같이 넘겨주세요!
                // (hitSound와 hitEffectPrefab은 부모인 Weapon 클래스에 이미 있습니다)
                Projectile p = proj.GetComponent<Projectile>();
                if (p != null)
                {
                    p.Initialize(damagePerProjectile, hitSound, hitEffectPrefab);
                }
            }
        }
    }
}