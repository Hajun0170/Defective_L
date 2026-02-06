using UnityEngine;
using System.Collections;

public class GatlingEnemy : MonoBehaviour
{
    [Header("Weapon Settings")]
    public GameObject bulletPrefab;   // 총알 프리팹 (EnemyProjectile 재활용 가능)
    public Transform firePoint;       // 발사 위치
    public GameObject visualBarrel;   // ★ 회전할 총열 부분 (없으면 비워도 됨)

    [Header("Pattern Settings")]
    public float attackRange = 12f;
    public float spinUpTime = 1.0f;   // 공격 전 예열 시간 (위잉~ 소리 날 때)
    public float burstDuration = 2.0f;// 몇 초 동안 쏠 건지
    public float fireRate = 0.1f;     // 연사 속도 (0.1초마다 발사)
    public float reloadTime = 3.0f;   // 재장전 시간
    
    [Header("Accuracy")]
    public float spreadAngle = 5f;    // 탄퍼짐 정도 (0이면 일직선)

    private Transform player;
    private bool isAttacking = false;
    private Vector3 originalScale;    // 좌우 반전용

    [Header("Aim Offset")]
    public float aimOffset = 0.75f;   // 플레이어 가슴 조준

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (player == null) return;

        // 1. 플레이어 바라보기 (좌우 반전)
        FlipTowardsPlayer();

        // 2. 총열 회전 (조준)
        RotateBarrelToPlayer();

        // 3. 거리 체크 및 공격
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // 1. 예열 (Spin Up) - 총열이 빠르게 돌기 시작하는 연출 가능
        // Debug.Log("개틀링: 위이잉... (예열)");
        
        // (선택) 예열하는 동안 총열을 돌리고 싶다면 여기서 코루틴을 하나 더 돌려도 됨
        float spinTimer = 0f;
        while(spinTimer < spinUpTime)
        {
            // 예열 중에는 총열만 뱅글뱅글 돔
            if(visualBarrel != null) visualBarrel.transform.Rotate(0, 0, 720 * Time.deltaTime);
            spinTimer += Time.deltaTime;
            yield return null;
        }

        // 2. 난사 (Burst Fire) - ★ 여기가 핵심 변경점!
        // Debug.Log("개틀링: 발사!!");
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

        // 3. 재장전 (Cooldown)
        // Debug.Log("개틀링: 과열! 식히는 중...");
        yield return new WaitForSeconds(reloadTime);

        isAttacking = false;
    }

    void FireBullet()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            // 플레이어 가슴 쪽 조준
            Vector3 targetPos = player.position + Vector3.up * aimOffset;
            Vector3 direction = (targetPos - firePoint.position).normalized;
            
            // 기본 각도
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // ★ 탄퍼짐 적용 (랜덤 오차)
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
        // 공격 중(난사 중)일 때는 조준을 멈추고 쏘던 방향으로 쏠지, 
        // 아니면 계속 따라가며 쏠지 결정해야 함. 
        // 여기서는 "계속 따라가며 쏘기"로 구현함. (싫으면 if(isAttacking) return; 추가)
        
        if (visualBarrel == null) return;

        Vector3 targetPos = player.position + Vector3.up * aimOffset;
        Vector3 direction = (targetPos - visualBarrel.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (transform.localScale.x < 0) angle += 180;

        // ★ 발사 중에는 총열 자체가 뺑뺑 돌기 때문에, 
        // RotateBarrelToPlayer가 Z축 회전을 덮어씌우면 안 됨.
        // 그래서 "부모 오브젝트(팔)"를 회전시키고, "자식(총열)"은 로컬로 회전시키는 게 베스트임.
        // 하지만 구조가 복잡해지니, 여기서는 단순하게 각도만 맞춤.
        // 만약 총열이 도는 연출이 중요하다면, visualBarrel의 부모인 'Arm'을 만들어서 그걸 이 함수로 돌리세요.
        visualBarrel.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}