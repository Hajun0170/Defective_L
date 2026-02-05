using UnityEngine;
using System;
using System.Collections; 

[CreateAssetMenu(fileName = "New Ranged", menuName = "Weapon/Ranged Weapon")]
public class RangedWeapon : Weapon
{
    [Header("Ranged Settings")]
    [SerializeField] protected int gaugeCost = 5; 

    [Header("Animation Settings")]
    public string skillTriggerName = "R_Skill_1";

    [Header("Firing Settings")]
    public int burstCount = 1;      // 개틀링: 6, 로켓/나이프: 1
    public float burstRate = 0.1f;  
    public float startDelay = 0f;   

    public int GetCurrentCost()
    {
        return gaugeCost;
    }

    // ★ PlayerAttack에서 호출하는 함수 (진입점)
    public override void UseSkill(Transform firePoint, PlayerStats playerStats, Action onComplete)
    {
        // 1. 코스트 계산
        int cost = GetCurrentCost();

        // 2. 게이지 확인
        if (playerStats.CurrentGauge >= cost)
        {
            // 3. 게이지 사용 (발사 시작 시 1회 차감)
            playerStats.UseGauge(cost);

            // 4. 교체 버프 소모
            playerStats.ConsumeSwapBuff();

            // 5. 연사 코루틴 시작
            playerStats.StartCoroutine(FireRoutine(firePoint, playerStats, onComplete));
        }
        else
        {
            Debug.Log($"[{weaponName}] 게이지 부족!");
            onComplete?.Invoke();
        }
    }

    // ★ 실제 발사 로직 (코루틴)
    protected IEnumerator FireRoutine(Transform firePoint, PlayerStats playerStats, Action onComplete)
    {
        // 선딜레이
        if (startDelay > 0) yield return new WaitForSeconds(startDelay);

        // 연사 횟수만큼 반복
        for (int i = 0; i < burstCount; i++)
        {
            // ★★★ [수정 핵심] 여기서 직접 Instantiate 하지 말고 SpawnProjectile을 부르세요! ★★★
            // 그래야 Projectile 스크립트의 Initialize()가 실행되어 날아갑니다.
            SpawnProjectile(firePoint, playerStats);

            // 2발 이상일 때만 대기 (로켓펀치는 1발이라 바로 끝남)
            if (burstCount > 1) yield return new WaitForSeconds(burstRate);
        }

        onComplete?.Invoke();
    }

    // ★ 발사체 생성 및 초기화 함수 (기존 코드 활용)
    protected virtual void SpawnProjectile(Transform firePoint, PlayerStats stats)
    {
        if (projectilePrefab != null)
        {
            bool isLookingLeft = firePoint.lossyScale.x < 0;
            Quaternion rotation = isLookingLeft ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
            
            // 1. 생성
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, rotation);
            
            // 2. 초기화 (이게 실행되어야 날아감!)
            Projectile p = proj.GetComponent<Projectile>();
            if (p != null)
            {
                // ★ [수정] Initialize 부를 때 'hitEffectPrefab'도 같이 넘겨주기!
                // (hitEffectPrefab은 Weapon 스크립트에 있는 변수입니다)
                p.Initialize(GetFinalDamage(stats), hitSound, hitEffectPrefab);
            }
        }
    }
    
    // (PerformAttack은 이제 안 쓰거나 UseSkill로 연결해도 됩니다)
    public override void PerformAttack(Transform firePoint, PlayerStats playerStats, Action onComplete)
    {
        UseSkill(firePoint, playerStats, onComplete);
    }
}