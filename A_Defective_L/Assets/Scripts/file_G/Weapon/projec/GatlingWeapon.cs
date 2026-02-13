using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Gatling", menuName = "Weapon/Gatling Gun")]
public class GatlingWeapon : Weapon // Weapon 상속. 연사로 나가는 투사체 코드
{
    [Header("Gatling Settings")]
    public int burstCount = 6;          // 발사 횟수 (현재는 6발. 경우에 따라 확장 고려)
    public float timeBetweenShots = 0.1f; // 발사 간격

    

    // Weapon의 PerformAttack을 재정의(Override) 
    public override void PerformAttack(Transform firePoint, PlayerStats playerStats, System.Action onComplete)
    {
        // ScriptableObject는 코루틴을 직접 못 돌려서, playerStats에게 요청
        playerStats.StartCoroutine(FireBurstRoutine(firePoint, playerStats, onComplete));
    }

    IEnumerator FireBurstRoutine(Transform firePoint, PlayerStats playerStats, System.Action onComplete)
    {
        for (int i = 0; i < burstCount; i++)
        {
            // 총알 생성
            if (projectilePrefab != null)
            {
                // 각도 등을 살짝 랜덤하게 줄 수도 있음(현재는 일직선으로 발사)
                Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            }

            // 발사 사운드/이펙트 (
            // AudioManager.Instance.PlaySfx("Gatling_Shot"); //현재는 비활성화

            // 대기
            yield return new WaitForSeconds(timeBetweenShots);
        }

        // 6발 다 쏘면 종료
        onComplete?.Invoke();
    }
}