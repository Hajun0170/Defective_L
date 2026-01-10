using UnityEngine;
using System; // Action 사용

[CreateAssetMenu(fileName = "New Ranged", menuName = "Weapon/Ranged Weapon")]
public class RangedWeapon : Weapon
{
    // [SerializeField] protected GameObject projectilePrefab; // <-- 부모(Weapon)에 있으니 삭제!
    
    [Header("Ranged Settings")]
    [SerializeField] protected int gaugeCost = 5; // 소모량

    public override void PerformAttack(Transform firePoint, PlayerStats playerStats, Action onComplete)
    {
        // 1. 게이지 사용 시도
        if (playerStats.UseGauge(gaugeCost))
        {
            // 성공 시 투사체 발사
            SpawnProjectile(firePoint, playerStats);
            Debug.Log($"[{weaponName}] 발사!");
        }
        else
        {
            Debug.Log($"[{weaponName}] 게이지 부족! 발사 실패.");
            // 실패했을 때 별도의 '찰칵' 소리나 UI 피드백을 넣을 수 있음
        }

        // 2. ★ 성공하든 실패하든, 행동이 끝났음을 보고해야 함!
        // (이게 없으면 게이지 부족할 때 플레이어가 굳어버림)
        onComplete?.Invoke();
    }

    // 투사체 생성 로직
    protected virtual void SpawnProjectile(Transform firePoint, PlayerStats stats)
    {
        // 캐릭터가 바라보는 방향(왼쪽/오른쪽) 체크
        bool isFlipped = firePoint.lossyScale.x < 0; // 혹은 PlayerMovement의 flip 상태 참조

        // 회전값 설정 (왼쪽이면 180도, 오른쪽이면 0도)
        Quaternion rotation = isFlipped ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
        
        // 투사체 생성 (부모 클래스의 projectilePrefab 사용)
        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, rotation);
            
            // 생성된 투사체에 데미지 주입 (Projectile 스크립트가 있다고 가정)
            // proj.GetComponent<Projectile>()?.Initialize(GetFinalDamage(stats));
        }
    }
}