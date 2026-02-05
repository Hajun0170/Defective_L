using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Melee", menuName = "Weapon/Melee Weapon")]
public class MeleeWeapon : Weapon
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 0.8f; // 공격 사거리
    [SerializeField] private LayerMask enemyLayer;     // 적 레이어
    [SerializeField] private int gaugeRecovery = 1;    // ★ 적중 시 회복할 게이지 양

    [Header("Visual Effects")]
    //public GameObject hitEffectPrefab; // ★ 여기에 이펙트 프리팹을 넣으세요
    public Vector2 effectOffset; // ★ [추가] 위치 미세 조정용 (X, Y)


    public override void PerformAttack(Transform firePoint, PlayerStats playerStats, Action onComplete)
    {
        // 1. 범위 내 적 감지 (원형 범위)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, attackRange, enemyLayer);

        // 2. 적이 한 명이라도 있으면?
        if (hitEnemies.Length > 0)
        {   
            // ★ [추가] 여기서 소리를 재생해야 합니다!
            if (hitSound != null && AudioManager.Instance != null)
            {
                 // AudioManager가 있으면 설정된 hitSound 재생
                AudioManager.Instance.PlaySFX(hitSound);
            }
            
            // A. 게이지 회복 (공격 1회당 한 번만 회복)
            // 만약 '맞은 적 수만큼' 회복하고 싶다면 foreach 안으로 옮기면 됩니다.
            playerStats.AddGauge(gaugeRecovery); 
            Debug.Log($"[{weaponName}] 적중! 게이지 {gaugeRecovery} 회복");
            int finalDamage = GetFinalDamage(playerStats); // 데미지 미리 계산

            // 플레이어가 보고 있는 방향 (1 or -1)
            float facingDir = Mathf.Sign(playerStats.transform.localScale.x);
            // B. 데미지 및 넉백 처리
            foreach (Collider2D enemy in hitEnemies)
            {
                // 넉백을 위해 '때린 사람의 위치(playerStats.transform)'를 같이 보냄
               // enemy.GetComponent<EnemyHealth>()?.TakeDamage(GetFinalDamage(playerStats), playerStats.transform);
                // A. 일반 몬스터인지 확인
                EnemyHealth normalMob = enemy.GetComponent<EnemyHealth>();
                if (normalMob != null)
                {
                    normalMob.TakeDamage(finalDamage, playerStats.transform);
                }
                // B. ★ 보스 몬스터인지 확인 (추가된 부분)
                else
                {
                    BossController boss = enemy.GetComponent<BossController>();
                    if (boss != null)
                    {
                        boss.TakeDamage(finalDamage);
                    }
                }

                // [탐지기 2] 데미지 로직 통과함?
                if (hitEffectPrefab != null)
                {

      

                    // 1. 위치 보정: 발밑(position) 대신 몸통 중앙(bounds.center) 사용
                    Vector3 spawnPos = enemy.bounds.center; 

                    // 2. ★ 오프셋 적용 (플레이어 방향에 따라 X축 반전)
                    // 플레이어가 오른쪽 볼 땐 offset.x 그대로, 왼쪽 볼 땐 반대로
                    Vector3 adjustedOffset = new Vector3(effectOffset.x * facingDir, effectOffset.y, 0);
                    
                    spawnPos += adjustedOffset;


                    // 2. 이펙트 생성 (변수에 담기)
                    GameObject effect = Instantiate(hitEffectPrefab, spawnPos, Quaternion.identity);

                    // 4. 좌우 반전 (기존 코드)
                    Vector3 effectScale = effect.transform.localScale;
                    effectScale.x = Mathf.Abs(effectScale.x) * facingDir;
                    effect.transform.localScale = effectScale;
                }
                 
              
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