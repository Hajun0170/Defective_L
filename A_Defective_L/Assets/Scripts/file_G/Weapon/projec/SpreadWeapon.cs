using UnityEngine;

// 1. 에셋 메뉴 추가 (파일 생성을 위해 필수)
[CreateAssetMenu(fileName = "New Spread", menuName = "Weapon/Spread Weapon")]
public class SpreadWeapon : RangedWeapon
{
    [Header("Spread Settings")]
    [SerializeField] private int projectileCount = 3; // 몇 갈래?
    [SerializeField] private float spreadAngle = 15f; // 퍼지는 각도

    // 부모(RangedWeapon)의 함수를 덮어씀
    protected override void SpawnProjectile(Transform firePoint, PlayerStats stats)
    {
        // 데미지 분산 (여러 발 맞으면 너무 세니까)
        int totalDamage = GetFinalDamage(stats);
        int damagePerProjectile = Mathf.Max(1, totalDamage / projectileCount);

        // 1. 캐릭터가 보고 있는 방향(각도) 계산
        // (Player가 뒤집혔다면 firePoint의 scale.x가 음수라고 가정)
        bool isFlipped = firePoint.lossyScale.x < 0;
        float baseAngle = isFlipped ? 180f : 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            // 2. 부채꼴 각도 계산
            // 예: 3발이면 -> -15도, 0도, +15도
            float angleOffset = spreadAngle * (i - (projectileCount - 1) / 2f);
            
            // 기준 각도에 더하기 (왼쪽을 볼 땐 각도가 뒤집히는 효과)
            // 왼쪽(180) + 15 = 195도(아래쪽), 180 - 15 = 165도(위쪽) -> 자연스럽게 퍼짐
            float currentAngle = baseAngle + angleOffset;
            
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);

            // 3. 생성
            if (projectilePrefab != null)
            {
                GameObject proj = Instantiate(projectilePrefab, firePoint.position, rotation);

                // ★ [핵심 수정] 회전값(Quaternion)을 방향 벡터(Vector2)로 변환
                // (회전된 상태에서 오른쪽(1,0)이 어디냐를 구함)
                Vector2 direction = rotation * Vector2.right;

                // 4. 초기화 (데미지와 방향 전달)
                proj.GetComponent<Projectile>()?.Initialize(damagePerProjectile, direction);
            }
        }
    }
}