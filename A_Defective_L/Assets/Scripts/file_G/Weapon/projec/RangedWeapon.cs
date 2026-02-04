using UnityEngine;
using System; 

[CreateAssetMenu(fileName = "New Ranged", menuName = "Weapon/Ranged Weapon")]
public class RangedWeapon : Weapon
{
    [Header("Ranged Settings")]
    [SerializeField] protected int gaugeCost = 5; 

/*
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
    */
    // 현재 레벨에 맞는 소모량 계산
    // 이제 레벨 상관없이 항상 기본 코스트 반환
    public int GetCurrentCost()
    {
        return gaugeCost;
    }

    public override void PerformAttack(Transform firePoint, PlayerStats playerStats, Action onComplete)
    {
       // 1. 소모량 계산
        int cost = GetCurrentCost();

        // 2. ★ [핵심] 게이지가 충분한지 확인! (이게 없어서 무한 발사된 것)
        if (playerStats.CurrentGauge >= cost)
        {
            // 3. 게이지 사용
            playerStats.UseGauge(cost);

            // 3. ★ [핵심] 직접 Instantiate 하지 말고, 가상 함수를 호출합니다!
            // 그래야 자식인 SpreadWeapon이 자기 방식대로 쏠 수 있습니다.
            SpawnProjectile(firePoint, playerStats);

            // 5. 원거리 무기는 "한 발" 쏘면 교체 버프 끝!
            playerStats.ConsumeSwapBuff();
        }
        else
        {
            // 게이지 부족! (딸깍 소리 등을 넣을 수 있음)
            Debug.Log("게이지 부족!");
        }

        // 6. 공격 종료 보고
        onComplete?.Invoke();
       }


    protected virtual void SpawnProjectile(Transform firePoint, PlayerStats stats)
    {
       if (projectilePrefab != null)
        {
            bool isLookingLeft = firePoint.lossyScale.x < 0;

            // 왼쪽을 보면 Z축으로 180도 회전, 아니면 기본(0도)
            Quaternion rotation = isLookingLeft ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
            // 1. 생성
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, rotation);
            
            // 2. ★ [누락되었던 부분] 초기화 함수를 꼭 불러야 날아갑니다!
            Projectile p = proj.GetComponent<Projectile>();
            if (p != null)
            {
                p.Initialize(GetFinalDamage(stats));
            }
        }
    }

    
}   