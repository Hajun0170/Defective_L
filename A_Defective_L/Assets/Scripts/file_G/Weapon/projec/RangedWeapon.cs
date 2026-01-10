using UnityEngine;
using System; 

[CreateAssetMenu(fileName = "New Ranged", menuName = "Weapon/Ranged Weapon")]
public class RangedWeapon : Weapon
{
    [Header("Ranged Settings")]
    [SerializeField] protected int gaugeCost = 5; 

    public override void PerformAttack(Transform firePoint, PlayerStats playerStats, Action onComplete)
    {
        if (playerStats.UseGauge(gaugeCost))
        {
            SpawnProjectile(firePoint, playerStats);
            // Debug.Log($"[{weaponName}] 발사!"); // 로그 너무 많으면 주석 처리
        }
        else
        {
            // Debug.Log($"[{weaponName}] 게이지 부족!");
        }

        onComplete?.Invoke();
    }

    protected virtual void SpawnProjectile(Transform firePoint, PlayerStats stats)
    {
        bool isFlipped = firePoint.lossyScale.x < 0; 
        Quaternion rotation = isFlipped ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
        
        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, rotation);
            
            // ★ [수정] 주석 해제! 
            // 이제 Initialize는 데미지(int) 하나만 받으므로 에러가 나지 않습니다.
            proj.GetComponent<Projectile>()?.Initialize(GetFinalDamage(stats));
        }
    }
}