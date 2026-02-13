using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Melee", menuName = "Weapon/Melee Weapon")]
public class MeleeWeapon : Weapon
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 0.8f; // 공격 사거리
    [SerializeField] private LayerMask enemyLayer;     // 적 레이어
    [SerializeField] private int gaugeRecovery = 1;    // 적중 시 회복할 게이지 양

    [Header("Visual Effects")]
        public Vector2 effectOffset; // 위치 미세 조정용 (X, Y)


    public override void PerformAttack(Transform firePoint, PlayerStats playerStats, Action onComplete)
    {
        // 범위 내 적 감지 (원형 범위)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, attackRange, enemyLayer);

        // 적이 한 명이라도 있는 경우
        if (hitEnemies.Length > 0)
        {   
            // 여기서 소리를 재생
            if (hitSound != null && AudioManager.Instance != null)
            {
                 // AudioManager가 있으면 설정된 hitSound 재생
                AudioManager.Instance.PlaySFX(hitSound);
            }
            
            // 게이지 회복 (공격 1회당 한 번만 회복)
            // '맞은 적 수만큼 회복하고 싶다면 foreach 안으로 옮기자. 현재는 1회만 회복됨'
            playerStats.AddGauge(gaugeRecovery); 
            Debug.Log($"[{weaponName}] 적중 게이지 {gaugeRecovery} 회복");
            int finalDamage = GetFinalDamage(playerStats); // 데미지 미리 계산

            // 플레이어가 보고 있는 방향 (1 or -1)
            float facingDir = Mathf.Sign(playerStats.transform.localScale.x);
            // 데미지 및 넉백 처리
            foreach (Collider2D enemy in hitEnemies)
            {
                // 넉백을 위해 때린 사람의 위치(playerStats.transform)를 같이 보냄
              // 일반 몬스터인지 확인
                EnemyHealth normalMob = enemy.GetComponent<EnemyHealth>();
                if (normalMob != null)
                {
                    normalMob.TakeDamage(finalDamage, playerStats.transform);
                }
                //  보스 몬스터인지 확인 
                else
                {
                    BossController boss = enemy.GetComponent<BossController>();
                    if (boss != null)
                    {
                        boss.TakeDamage(finalDamage);
                    }
                }

                // 데미지 로직 통과 여부
                if (hitEffectPrefab != null)
                {

                    // 위치 보정: 발밑 대신 몸통 중앙 사용
                    Vector3 spawnPos = enemy.bounds.center; 

                    // 오프셋 적용 플레이어 방향에 따라 X축 반전
                    // 플레이어가 오른쪽 볼 땐 offset.x 그대로, 왼쪽 볼 땐 반대로
                    Vector3 adjustedOffset = new Vector3(effectOffset.x * facingDir, effectOffset.y, 0);
                    
                    spawnPos += adjustedOffset;


                    //이펙트 생성 (변수에 담기)
                    GameObject effect = Instantiate(hitEffectPrefab, spawnPos, Quaternion.identity);

                    //좌우 반전
                    Vector3 effectScale = effect.transform.localScale;
                    effectScale.x = Mathf.Abs(effectScale.x) * facingDir;
                    effect.transform.localScale = effectScale;
                }                 
           }
        }
        else
        {
            // 빗나갔을 때 로그. 빌드 뽑아서 비활성화
        }

        // 공격 완료
        onComplete?.Invoke();
    }
}