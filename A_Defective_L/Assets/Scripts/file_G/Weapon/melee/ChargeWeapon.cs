using System.Collections;
using UnityEngine;
using System; // Action 사용

[CreateAssetMenu(fileName = "ChargeWeapon", menuName = "Weapon/Charge Weapon")]
public class ChargeWeapon : Weapon //충전형 무기를 만드려 했었는데, 물리엔진 오류로 사용하지 않음
{
    [Header("Charge Settings")]
    public float chargeTime = 0.8f;   // 기 모으는 시간
    public float attackRange = 1.5f;  // 공격 범위
    public LayerMask enemyLayer;      // 적 레이어

    // 부모 함수 오버라이드
    public override void PerformAttack(Transform attackPoint, PlayerStats playerStats, Action onComplete)
    {
        // PlayerStats를 통해 코루틴 실행
        playerStats.StartCoroutine(ProcessCharge(attackPoint, playerStats, onComplete));
    }

    private IEnumerator ProcessCharge(Transform attackPoint, PlayerStats playerStats, Action onComplete)
    {
        Debug.Log($"[{weaponName}] 기 모으는 중... ({chargeTime}초)");

        // 차징 대기
        yield return new WaitForSeconds(chargeTime);

        // 공격 판정
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        int finalDamage = GetFinalDamage(playerStats);

        bool hitSuccess = false;
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(finalDamage, playerStats.transform);
            hitSuccess = true;
            // 이펙트 생성 필요하면 삽입
        }

        if (hitSuccess)
        {
            playerStats.AddGauge(1); // 게이지 충전
            Debug.Log($"[{weaponName}]  {hitEnemies.Length}명 타격.");
        }
        else
        {
            Debug.Log($"[{weaponName}] 미스");
        }

        // 공격 끝
        onComplete?.Invoke();
    }
}