using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Gatling", menuName = "Weapon/Gatling Gun")]
public class GatlingWeapon : Weapon // Weapon 상속!
{
    [Header("Gatling Settings")]
    public int burstCount = 6;          // 발사 횟수 (6발)
    public float timeBetweenShots = 0.1f; // 발사 간격

    

    // Weapon의 PerformAttack을 "재정의(Override)" 합니다.
    public override void PerformAttack(Transform firePoint, PlayerStats playerStats, System.Action onComplete)
    {
        // ScriptableObject는 코루틴을 직접 못 돌리므로, playerStats(MonoBehaviour)에게 부탁합니다.
        playerStats.StartCoroutine(FireBurstRoutine(firePoint, playerStats, onComplete));
    }

    IEnumerator FireBurstRoutine(Transform firePoint, PlayerStats playerStats, System.Action onComplete)
    {
        for (int i = 0; i < burstCount; i++)
        {
            // 1. 총알 생성
            if (projectilePrefab != null)
            {
                // 각도 등을 살짝 랜덤하게 줄 수도 있음
                Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            }

            // 2. 발사 사운드/이펙트 (필요하면 추가)
            // AudioManager.Instance.PlaySfx("Gatling_Shot");

            // 3. 대기
            yield return new WaitForSeconds(timeBetweenShots);
        }

        // 6발 다 쏘면 끝났다고 보고
        onComplete?.Invoke();
    }
}