using System.Collections;
using UnityEngine;

public class ChargeWeapon : Weapon
{
    [Header("Charge Settings")]
    [SerializeField] private float chargeTime = 0.8f; // 차징 시간
    [SerializeField] private float attackRange = 1.0f;
    [SerializeField] private LayerMask enemyLayer;
    
    private bool isCharging = false;

    public override void PerformAttack(Transform firePoint, PlayerStats playerStats)
    {
        // 이미 차징 중이거나 쿨타임 중이면 무시
        if (isCharging || Time.time < nextAttackTime) return;

        // 차징 코루틴 시작
        StartCoroutine(ProcessCharge(firePoint, playerStats));
    }

    private IEnumerator ProcessCharge(Transform firePoint, PlayerStats playerStats)
    {
        isCharging = true;
        Debug.Log($"[{weaponName}] 차징 시작... (움직이지 마세요)");
        
        // (선택 사항) 차징 중 이펙트나 사운드 재생
        
        // 설정한 시간만큼 대기
        yield return new WaitForSeconds(chargeTime);

        // 공격 판정 실행
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, attackRange, enemyLayer);
        int finalDamage = GetFinalDamage(playerStats);

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(finalDamage);
            
            // 차징 공격도 적중 시 게이지 충전
            playerStats.AddGauge(1); 
        }
        Debug.Log($"[{weaponName}] 발사! 데미지: {finalDamage}");

        // 상태 초기화 및 쿨타임 적용
        isCharging = false;
        nextAttackTime = Time.time + attackRate;
    }
    
    private void OnDrawGizmosSelected() 
    { 
        Gizmos.color = Color.yellow; 
        Gizmos.DrawWireSphere(transform.position, attackRange); 
    }
}