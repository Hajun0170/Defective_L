using UnityEngine;
using System.Collections;

public class RocketBoss : MonoBehaviour
{
    [Header("Rocket Settings")]
    public GameObject rocketProjectilePrefab; 
    public Transform firePoint;               
    public GameObject visualRocketArm;        // 회전시킬 팔 오브젝트

    [Header("Pattern Settings")]
    public float attackRange = 10f;
    public float castTime = 1.0f;   // 조준 시간
    public float reloadTime = 3.0f; // 재장전 시간

    [Header("★ [추가] Movement Settings")]
    public bool canFlyAndChase = false; // 켜면 추격, 끄면 고정
    public float moveSpeed = 3.0f;
    public float stopDistance = 5.0f;   // 플레이어와 유지할 거리

    private Transform player;
    private bool isAttacking = false;
    private SpriteRenderer spriteRenderer; // 보스 몸체 뒤집기용 (없으면 transform.scale 사용)

    // ★ [추가] 원래 크기를 기억할 변수
    private Vector3 originalScale;

    [Header("Aim Offset")]
    public float aimOffset = 0.75f; // ★ [추가] 0.75만큼 위를 조준함 (가슴 높이)
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ★ [추가] 시작할 때 인스펙터에 설정된 크기를 저장해둠!
        originalScale = transform.localScale;

        if (visualRocketArm != null) visualRocketArm.SetActive(true);
    }

    private void Update()
    {
        if (player == null) return;

        // 1. 플레이어 바라보기 (좌우 반전)
        FlipTowardsPlayer();

        // 2. 팔 회전 (조준)
        RotateArmToPlayer();

        float distance = Vector2.Distance(transform.position, player.position);

        // 3. 이동 로직 (공격 중이 아닐 때만 이동)
        if (canFlyAndChase && !isAttacking)
        {
            if (distance > stopDistance)
            {
                // 플레이어 쪽으로 날아감
                transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
        }

        // 4. 공격 로직
        // 사거리 안이고, 공격 중이 아니고, 로켓이 장전되어 있다면
        if (distance <= attackRange && !isAttacking && visualRocketArm.activeSelf)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    // ★ 몸체 좌우 반전 함수
   void FlipTowardsPlayer()
    {
        // 원래 크기의 X값 절대값 (혹시 -3으로 시작했을 수도 있으니)
        float sizeX = Mathf.Abs(originalScale.x);

        if (player.position.x > transform.position.x)
        {
            // 오른쪽 볼 때: 원래 크기 그대로 (+X)
            transform.localScale = new Vector3(sizeX, originalScale.y, originalScale.z);
        }
        else
        {
            // 왼쪽 볼 때: X만 뒤집기 (-X)
            transform.localScale = new Vector3(-sizeX, originalScale.y, originalScale.z);
        }
    }

   // ★ 팔 회전 (조준) 수정
    void RotateArmToPlayer()
    {
        if (visualRocketArm == null) return;

        // ★ [수정] 발바닥 대신 가슴 쪽을 바라보게 함
        Vector3 targetPos = player.position + Vector3.up * aimOffset;
        
        Vector3 direction = (targetPos - visualRocketArm.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (transform.localScale.x < 0)
        {
            angle += 180; 
        }

        visualRocketArm.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // 1. 뜸 들이기 (이때도 Update에서 계속 조준함)
        yield return new WaitForSeconds(castTime);

        // 2. 발사!
        FireRocket();

        // 3. 재장전 대기
        yield return new WaitForSeconds(reloadTime);

        // 4. 재장전 완료
        if (visualRocketArm != null) visualRocketArm.SetActive(true);
        
        isAttacking = false;
    }

   // ★ 발사 로직 수정
    void FireRocket()
    {
        if (visualRocketArm != null) visualRocketArm.SetActive(false);

        if (rocketProjectilePrefab != null && firePoint != null)
        {
            // ★ [수정] 발사 각도도 가슴 쪽을 향하게 계산
            Vector3 targetPos = player.position + Vector3.up * aimOffset;

            Vector3 direction = (targetPos - firePoint.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            Instantiate(rocketProjectilePrefab, firePoint.position, rotation);
            Debug.Log("보스: 로켓 발사!");
        }
    }
}