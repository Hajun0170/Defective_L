using UnityEngine;

public class MeleeWeapon : Weapon
{
    [Header("Melee Settings")]
    [SerializeField] protected float attackRange = 0.8f;
    [SerializeField] protected LayerMask enemyLayer;

    public override void PerformAttack(Transform firePoint, PlayerStats playerStats)
    {
        // 쿨타임 체크
        if (Time.time < nextAttackTime) return;

        // 공격 범위 내의 적 감지
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, attackRange, enemyLayer);
        
        // 적이 한 명이라도 맞았는지 확인
        if (hitEnemies.Length > 0)
        {
            // 데미지 계산 (버프 적용)
            int finalDamage = GetFinalDamage(playerStats);

            foreach (Collider2D enemy in hitEnemies)
            {
                // 적에게 데미지 전달
                enemy.GetComponent<EnemyHealth>()?.TakeDamage(finalDamage);
                
                // 타격 성공 시 게이지 1 충전
                playerStats.AddGauge(1);
            }
        }

        // 다음 공격 시간 설정
        nextAttackTime = Time.time + attackRate;
    }
    
    // 에디터에서 공격 범위를 눈으로 확인하기 위한 코드
    private void OnDrawGizmosSelected() 
    { 
        Gizmos.color = Color.red; 
        Gizmos.DrawWireSphere(transform.position, attackRange); 
    }
}