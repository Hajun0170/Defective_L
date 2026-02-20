using UnityEngine;
using System.Collections;

public class GatlingEnemy : MonoBehaviour
{
    [Header("Weapon Settings")]
    public GameObject bulletPrefab;   // 총알 프리팹 (EnemyProjectile 재활용)
    public Transform firePoint;       // 발사 위치
    public GameObject visualBarrel;   // 회전할 총 부분 (시각적으로 표시하는 부분. 몬스터 모션에 회전 총을 삽입했음)

    [Header("Pattern Settings")]
    public float attackRange = 12f;
    public float spinUpTime = 1.0f;   // 공격 전 예열 시간 
    public float burstDuration = 2.0f;// 지속 시간
    public float fireRate = 0.1f;     // 0.1초마다 연사로 발사
    public float reloadTime = 3.0f;   // 재장전 시간
    
    [Header("Accuracy")]
    public float spreadAngle = 5f;    // 탄퍼짐 정도 (0이면 일직선, 플레이어가 대처를 못해서 게임 상으로는 일직선으로 발사됨)

    private Transform player;
    private bool isAttacking = false;
    private Vector3 originalScale;    // 좌우 반전용

    [Header("Aim Offset")]
    public float aimOffset = 0.75f;   // 플레이어 몸통 조준

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (player == null) return;

        // 플레이어 쳐다봄 (좌우 반전)
        FlipTowardsPlayer();

        // 총열 회전 (조준)
        RotateBarrelToPlayer();

        // 거리 체크 및 공격
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        float spinTimer = 0f;
        while(spinTimer < spinUpTime)
        {
            // 예열 중에는 총열만 돌아감(모션 삽입)
            if(visualBarrel != null) visualBarrel.transform.Rotate(0, 0, 720 * Time.deltaTime);
            spinTimer += Time.deltaTime;
            yield return null;
        }

        // 발사
        float burstTimer = 0f;
        while (burstTimer < burstDuration)
        {
            FireBullet();
            
            // 총열 회전 연출 (쏠 때도 돔)
            if(visualBarrel != null) visualBarrel.transform.Rotate(0, 0, 1000 * Time.deltaTime);

            // 다음 발사까지 대기
            yield return new WaitForSeconds(fireRate); 
            burstTimer += fireRate;
        }

        // 재장전
        yield return new WaitForSeconds(reloadTime);

        isAttacking = false;
    }

    void FireBullet()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            // 플레이어 몸통 조준
            Vector3 targetPos = player.position + Vector3.up * aimOffset;
            Vector3 direction = (targetPos - firePoint.position).normalized;
            
            // 기본 각도
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 탄퍼짐 적용 (랜덤 오차를 두어 사방으로 퍼짐)
            float randomSpread = Random.Range(-spreadAngle, spreadAngle);
            Quaternion rotation = Quaternion.AngleAxis(angle + randomSpread, Vector3.forward);

            // 생성
            Instantiate(bulletPrefab, firePoint.position, rotation);
        }
    }

    void FlipTowardsPlayer()
    {
        float sizeX = Mathf.Abs(originalScale.x);
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(sizeX, originalScale.y, originalScale.z);
        else
            transform.localScale = new Vector3(-sizeX, originalScale.y, originalScale.z);
    }

    void RotateBarrelToPlayer()
    {
        // 계속 따라가며 쏨. (싫으면 if(isAttacking) return; 추가해서 막는 방법도 존재)
        
        if (visualBarrel == null) return;

        Vector3 targetPos = player.position + Vector3.up * aimOffset;
        Vector3 direction = (targetPos - visualBarrel.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (transform.localScale.x < 0) angle += 180;

        // 단순하게 각도만 맞춤. 구조 복잡해지는 문제.
        visualBarrel.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}