using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Melee", menuName = "Weapon/Melee Weapon")]
public class MeleeWeapon : Weapon
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 0.8f; // 공격 사거리
    [SerializeField] private LayerMask enemyLayer;     // 적 레이어
    [SerializeField] private int gaugeRecovery = 1;    // ★ 적중 시 회복할 게이지 양

    public override void PerformAttack(Transform firePoint, PlayerStats playerStats, Action onComplete)
    {
        // 1. 범위 내 적 감지 (원형 범위)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, attackRange, enemyLayer);

        // 2. 적이 한 명이라도 있으면?
        if (hitEnemies.Length > 0)
        {
            // A. 게이지 회복 (공격 1회당 한 번만 회복)
            // 만약 '맞은 적 수만큼' 회복하고 싶다면 foreach 안으로 옮기면 됩니다.
            playerStats.AddGauge(gaugeRecovery); 
            Debug.Log($"[{weaponName}] 적중! 게이지 {gaugeRecovery} 회복");

            // B. 데미지 및 넉백 처리
            foreach (Collider2D enemy in hitEnemies)
            {
                // 넉백을 위해 '때린 사람의 위치(playerStats.transform)'를 같이 보냄
                enemy.GetComponent<EnemyHealth>()?.TakeDamage(GetFinalDamage(playerStats), playerStats.transform);
                
                // (선택) 타격 이펙트 생성
                // Instantiate(hitEffect, enemy.transform.position, Quaternion.identity);
            }
        }
        else
        {
            // (선택) 빗나갔을 때 로그
            // Debug.Log("허공을 갈랐다...");
        }

        // 3. 공격 완료 보고
        onComplete?.Invoke();
    }
}