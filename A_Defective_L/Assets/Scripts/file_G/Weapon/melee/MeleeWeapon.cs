using UnityEngine;
using System; // Action 사용

[CreateAssetMenu(fileName = "New Melee", menuName = "Weapon/Melee Weapon")]
public class MeleeWeapon : Weapon
{
    [Header("Melee Settings")]
    public float attackRange = 0.8f; // ★ Public으로 변경 (PlayerAttack에서 기즈모 그릴 때 필요)
    public LayerMask enemyLayer;

    // 부모의 바뀐 형식을 따라 매개변수 수정 (Action onComplete 추가)
    public override void PerformAttack(Transform firePoint, PlayerStats playerStats, Action onComplete)
    {
        // ★ 쿨타임 체크(Time.time < nextAttackTime)는 제거함!
        // (PlayerAttack 스크립트에서 이미 체크하고 들어왔기 때문)

        // 1. 공격 범위 내의 적 감지
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, attackRange, enemyLayer);
        
        // 2. 적이 한 명이라도 맞았는지 확인
        if (hitEnemies.Length > 0)
        {
            // 데미지 계산 (부모 클래스 함수 사용)
            int finalDamage = GetFinalDamage(playerStats);

            foreach (Collider2D enemy in hitEnemies)
            {
                // 적에게 데미지 전달
                enemy.GetComponent<EnemyHealth>()?.TakeDamage(finalDamage);
                
                // 타격 성공 시 게이지 1 충전
                playerStats.AddGauge(1);
            }
            Debug.Log($"[{weaponName}] 타격 성공! {hitEnemies.Length}명");
        }
        else
        {
            // 허공에 휘두름 (소리 재생 등 가능)
            // Debug.Log($"[{weaponName}] 허공 가르기");
        }

        // 3. ★ "공격 끝났음" 보고 (일반 공격은 즉시 종료)
        onComplete?.Invoke();
    }
}