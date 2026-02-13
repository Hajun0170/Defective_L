using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    // 발사된 투사체들을 담아둘 리스트
private List<GameObject> activeProjectiles = new List<GameObject>();
    
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

        // [체크] 대기 시간이 끝난 직후 보스가 이미 죽었거나 비활성화되었다면 중단!
    if (this == null || !gameObject.activeInHierarchy) yield break;

        // 2. 발사!
        FireRocket();

        // 3. 재장전 대기
        yield return new WaitForSeconds(reloadTime);

        // 4. 재장전 완료
        //if (visualRocketArm != null) visualRocketArm.SetActive(true);
        // [체크] 재장전 시점에도 보스가 살아있을 때만 팔을 다시 켬
    if (this != null && visualRocketArm != null)
    {
        visualRocketArm.SetActive(true);
    }
        
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

           // Instantiate(rocketProjectilePrefab, firePoint.position, rotation);

           // 생성된 투사체를 변수에 담고 리스트에 추가
        GameObject projectile = Instantiate(rocketProjectilePrefab, firePoint.position, rotation);

        

        activeProjectiles.Add(projectile);

        // ★ [추가] 리스트 청소 로직 (리스트에 쌓인 null 제거)
            // 발사할 때마다 한 번씩 죽은 미사일들을 리스트에서 치워줍니다.
            activeProjectiles.RemoveAll(item => item == null);
            Debug.Log("보스: 로켓 발사!");
        }
    }

    

// ★ [보강] OnDisable에서도 처리 (씬 전환이나 비활성화 시 유령 판정 방지)
    private void OnDisable()
    {
        ClearAllProjectiles();
    }

    // RocketBoss.cs 하단에 추가
private void OnDestroy()
    {
        ClearAllProjectiles();
    }

    
    // ★ [분리] 중복되는 삭제 로직을 하나로 묶음
    private void ClearAllProjectiles()
    {
        StopAllCoroutines();
        
        if (activeProjectiles != null)
        {
            foreach (GameObject proj in activeProjectiles)
            {
                if (proj != null) 
            {
                // 1. 물리 판정부터 즉시 제거 (가장 빠름)
                if (proj.TryGetComponent(out Collider2D col)) col.enabled = false;
                
                // 2. 시각적으로 즉시 숨김
                proj.SetActive(false);
                
                // 3. 실제 파괴 명령
                Destroy(proj);
            }
            }
            activeProjectiles.Clear();
        }
// 보스의 팔 오브젝트도 확실히 비활성화
        if (visualRocketArm != null) visualRocketArm.SetActive(false);
    }
}