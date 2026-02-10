using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BossController))]
public class FinalBossAI : MonoBehaviour
{
    [Header("1. 거리 및 감지 설정")]
    public float detectionRange = 10.0f;
    public float attackRange = 2.5f;
    public Vector2 centerOffset = new Vector2(0, 1.0f);
    public float heightCheckThreshold = 3.0f; // 플레이어가 이만큼 더 높으면 견제

    [Header("2. 이동 설정")]
    public float moveSpeed = 3.0f;
    public float dashSpeed = 12.0f;
    public float backstepSpeed = 8.0f;
    public LayerMask wallLayer; // 벽 레이어 (필수 설정!)

    // ★ [추가] 벽 감지 레이저의 시작 위치 조정 (발바닥보다 위로!)
    public Vector2 wallCheckOffset = new Vector2(0, 1.5f); 
    public float wallCheckDistance = 3.0f; // 벽 감지 거리
    

    [Header("3. 페이즈 설정")]
    [Range(0, 1)]
    public float phase2Threshold = 0.3f;
    private bool isPhase2 = false;

    [Header("4. 공격 오브젝트")]
    public GameObject meleeHitBox;
    public GameObject swordPrefab;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("5. 쿨타임")]
    public float patternDelay = 1.5f;

    private BossController status;
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;

    private bool isActing = false;
    private float cooldownTimer = 0f;

    void Awake()
    {
        status = GetComponent<BossController>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
       // rb.bodyType = RigidbodyType2D.Kinematic; 물리판정 문제로 인해 Dynamic으로 변경 
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (meleeHitBox != null) meleeHitBox.SetActive(false);
    }

    void Update()
    {
        if (status.isDead || !status.isIntroFinished)
        {
            StopMoving();
            return;
        }

        if (isActing) return;

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            StopMoving();
            FlipSprite();
            return;
        }

        if (player == null) return;

        Vector2 centerPos = (Vector2)transform.position + centerOffset;
        float dist = Vector2.Distance(centerPos, player.position);
        float heightDiff = player.position.y - transform.position.y; // 높이 차이 계산

        // 1. 플레이어가 너무 높이 있으면 원거리 견제
        if (heightDiff > heightCheckThreshold && dist < detectionRange)
        {
             StopMoving();
             StartCoroutine(ExecuteHighAnglePattern()); // 상단 견제 패턴
        }
        // 2. 일반적인 거리 판단
        else if (dist > detectionRange)
        {
            StopMoving();
        }
        else if (dist > attackRange)
        {
            MoveToPlayer();
        }
        else
        {
            StopMoving();
            StartCoroutine(ExecutePattern());
        }

        CheckPhase();
    }

    IEnumerator ExecutePattern()
    {
        isActing = true;
        FlipSprite();

        int rand = Random.Range(0, 100);

        if (isPhase2)
        {
            if (rand < 40) yield return StartCoroutine(Pattern_BackstepGatling());
            else if (rand < 70) yield return StartCoroutine(Pattern_DashAttack(true));
            else yield return StartCoroutine(Pattern_DashAttack(false));
        }
        else
        {
            if (rand < 40) yield return StartCoroutine(Pattern_BackstepSword());
            else yield return StartCoroutine(Pattern_MeleeAttack());
        }

        cooldownTimer = patternDelay;
        isActing = false;
    }

    // ★ [추가] 플레이어가 높이 있을 때 사용하는 패턴
    IEnumerator ExecuteHighAnglePattern()
    {
        isActing = true;
        FlipSprite();
        
        Debug.Log("높은 곳의 플레이어 견제!");

        // 2페이즈면 개틀링, 1페이즈면 칼 던지기
        if (isPhase2)
        {
             yield return StartCoroutine(Pattern_BackstepGatling());
        }
        else
        {
             yield return StartCoroutine(Pattern_BackstepSword());
        }
        
        cooldownTimer = patternDelay;
        isActing = false;
    }

    // =========================================================
    // ⚔️ 공격 패턴
    // =========================================================

    IEnumerator Pattern_MeleeAttack()
    {
        yield return new WaitForSeconds(0.4f);
        anim.SetTrigger("D_A");
        if (meleeHitBox != null) meleeHitBox.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (meleeHitBox != null) meleeHitBox.SetActive(false);
        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator Pattern_BackstepSword()
    {
        Vector2 backDir = (transform.position - player.position).normalized;
        backDir.y = 0;
        
      // ★ [수정] 오프셋 변수 적용 (높이 조절 가능)
        Vector2 rayStart = (Vector2)transform.position + wallCheckOffset;


        bool hitWall = Physics2D.Raycast(rayStart, backDir, 2.0f, wallLayer);
       Debug.DrawRay(rayStart, backDir * wallCheckDistance, hitWall ? Color.red : Color.green, 2.0f);

        if (!hitWall)
        {
            rb.linearVelocity = backDir * backstepSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        anim.SetTrigger("Back");

        yield return new WaitForSeconds(0.5f);
        StopMoving();

        anim.SetTrigger("T_Knife");
        yield return new WaitForSeconds(0.2f);

        if (swordPrefab != null)
        {
            GameObject sword = Instantiate(swordPrefab, firePoint.position, Quaternion.identity);
            SetupProjectile(sword);
        }
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator Pattern_BackstepGatling()
    {
        Vector2 backDir = (transform.position - player.position).normalized;
        backDir.y = 0;

        Vector2 rayStart = (Vector2)transform.position + wallCheckOffset;

     

        bool hitWall = Physics2D.Raycast(rayStart, backDir, 2.0f, wallLayer);
        Debug.DrawRay(rayStart, backDir * wallCheckDistance, hitWall ? Color.red : Color.green, 2.0f);

        if (!hitWall) rb.linearVelocity = backDir * backstepSpeed;
        else rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Back");

        yield return new WaitForSeconds(0.7f);
        StopMoving();

        anim.SetTrigger("Gat");
        for (int i = 0; i < 6; i++)
        {
            if (bulletPrefab != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
                SetupProjectile(bullet);
            }
            yield return new WaitForSeconds(0.12f);
        }
        yield return new WaitForSeconds(1.0f);
    }

    IEnumerator Pattern_DashAttack(bool withBackstep)
    {
        if (withBackstep)
        {
            Vector2 backDir = (transform.position - player.position).normalized;
            backDir.y = 0;

           // ★ [수정] 오프셋 적용
            Vector2 rayStart = (Vector2)transform.position + wallCheckOffset;
            bool hitWall = Physics2D.Raycast(rayStart, backDir, wallCheckDistance, wallLayer);
            Debug.DrawRay(rayStart, backDir * wallCheckDistance, hitWall ? Color.red : Color.green, 2.0f);

            if (!hitWall) rb.linearVelocity = backDir * backstepSpeed;
            else rb.linearVelocity = Vector2.zero;

            anim.SetTrigger("Back");
            yield return new WaitForSeconds(0.4f);
        }

        StopMoving();
        yield return new WaitForSeconds(0.4f);

        Vector2 dashDir = (player.position - transform.position).normalized;
        dashDir.y = 0;
        rb.linearVelocity = dashDir * dashSpeed;
        anim.SetTrigger("D_A");

        if (meleeHitBox != null) meleeHitBox.SetActive(true);
        yield return new WaitForSeconds(0.6f);

        StopMoving();
        if (meleeHitBox != null) meleeHitBox.SetActive(false);
        yield return new WaitForSeconds(0.3f);
    }

    void CheckPhase()
    {
        if (isPhase2) return;
        if (status.GetHealthPercentage() <= phase2Threshold)
        {
            StartCoroutine(PhaseChangeRoutine());
        }
    }

    IEnumerator PhaseChangeRoutine()
    {
        isPhase2 = true;
        isActing = true;
        StopMoving();

        Debug.Log("🔥 2페이즈 각성!");
        moveSpeed *= 1.2f;
        patternDelay *= 0.7f;
        yield return new WaitForSeconds(2.0f);
        isActing = false;
    }

    void MoveToPlayer()
    {
        FlipSprite();
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
        anim.SetBool("IsWalk", true);
    }

    void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("IsWalk", false);
    }

    void FlipSprite()
    {
        float sizeX = Mathf.Abs(transform.localScale.x);
        float sizeY = transform.localScale.y;
        float sizeZ = transform.localScale.z;

        if (player.position.x > transform.position.x)
        {
            // 플레이어가 오른쪽 -> 보스도 오른쪽 봄 (Scale.x가 양수여야 함)
            transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
        }
        else
        {
            // 플레이어가 왼쪽 -> 보스도 왼쪽 봄 (Scale.x가 음수여야 함)
            transform.localScale = new Vector3(-sizeX, sizeY, sizeZ);
        }
    }

    // ★ [핵심 수정] EnemyProjectile 스크립트와 호환되도록 "회전"을 시킴
    /*
    void SetupProjectile(GameObject proj)
    {
        // 1. 보스가 왼쪽을 보고 있는지 확인 (Scale X가 음수면 왼쪽)
        bool isFacingLeft = transform.localScale.x < 0;

        // 2. EnemyProjectile은 "오른쪽(Vector3.right)"으로만 날아갑니다.
        // 따라서 왼쪽으로 쏘려면 투사체 자체를 180도 돌려야 합니다.
        if (isFacingLeft)
        {
            // Y축을 기준으로 180도 회전 (뒤로 돌아서게 함)
            proj.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            // 오른쪽이면 0도 (정면)
            proj.transform.rotation = Quaternion.identity;
        }

        // 3. 물리 속도 제거 (Translate와 충돌 방지)
        Rigidbody2D pRb = proj.GetComponent<Rigidbody2D>();
        if (pRb != null) pRb.linearVelocity = Vector2.zero; 
    }
    */
    // ★ [핵심 수정] 투사체가 플레이어를 향하도록 회전값 설정
   // ★ [수정] 플레이어의 발바닥이 아닌 '몸통'을 조준하도록 수정
    void SetupProjectile(GameObject proj)
    {
        // 1. 조준 높이 보정값 (이 값을 올리면 더 위로 쏩니다)
        float aimOffsetY = 1.5f; // 1.0f 정도면 보통 가슴~머리 높이입니다.

        // 2. 플레이어 위치 + 오프셋을 더해 '타겟 지점'을 만듦
        Vector3 targetPos = player.position + new Vector3(0, aimOffsetY, 0);

        // 3. 발사 위치(FirePoint)에서 타겟 지점(targetPos)을 향하는 방향 계산
        Vector3 targetDir = (targetPos - firePoint.position).normalized;

        // 4. 각도 계산 (Atan2)
        float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

        // 5. 투사체 회전 적용
        // EnemyProjectile 스크립트가 '오른쪽(Right)'으로 전진하므로,
        // 회전만 시켜주면 알아서 해당 방향으로 날아갑니다.
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 6. 물리 속도 제거 (Translate와 충돌 방지)
        Rigidbody2D pRb = proj.GetComponent<Rigidbody2D>();
        if (pRb != null) 
        {
            pRb.linearVelocity = Vector2.zero; 
            pRb.angularVelocity = 0f; // 회전 관성도 제거
        }
    }

   // ★ [수정] Gizmos 길이 변수 적용
    void OnDrawGizmosSelected()
    {   
        // 1. 감지 범위
        Vector3 centerPos = transform.position + (Vector3)centerOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPos, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centerPos, attackRange);

        // 2. 벽 감지 레이저 (파란색)
        Gizmos.color = Color.blue;
        Vector3 wallRayStart = transform.position + (Vector3)wallCheckOffset;
        
        // ★ 2.0f 대신 wallCheckDistance 변수를 사용하도록 수정!
        // 왼쪽으로 쏘는 선
        Gizmos.DrawLine(wallRayStart, wallRayStart + Vector3.left * wallCheckDistance);
        // 오른쪽으로 쏘는 선
        Gizmos.DrawLine(wallRayStart, wallRayStart + Vector3.right * wallCheckDistance);
        
        Gizmos.DrawSphere(wallRayStart, 0.2f);
    }

    // ★ [추가] 외부(BossController)에서 호출할 함수: "야! 동작 그만!"
    public void StopAllPatterns()
    {
        // 1. 실행 중인 공격 패턴(개틀링, 칼던지기 등) 즉시 종료
        StopAllCoroutines(); 

        // 2. 이동 물리력 제거 (미끄러짐 방지)
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 3. 걷기 애니메이션 해제
        if (anim != null) anim.SetBool("IsWalk", false);

        // 4. 상태 초기화
        isActing = false;
        
        // 5. 더 이상 Update가 돌지 않도록 스크립트 자체를 비활성화 (확실한 마무리)
        this.enabled = false; 

        Debug.Log("🚫 AI: 모든 패턴 강제 종료됨");
    }

}