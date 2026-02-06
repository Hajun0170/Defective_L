using System.Collections;
using UnityEngine;

public class BossWallPattern : MonoBehaviour
{
    // ... (기존 설정 변수들: moveSpeed, detectRange 등등 그대로 유지) ...
    [Header("Basic Stats")]
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float detectRange = 8.0f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private int attackDamage = 1;

    [Header("Combat Settings")]
    [SerializeField] private float attackRate = 1.5f;
    [SerializeField] private float attackAfterDelay = 1.0f;
    [SerializeField] private Vector2 rangeCenterOffset;

    [Header("Wall Slam Pattern")]
    [SerializeField] private float phaseTwoRatio = 0.5f; // 체력 50%
    [SerializeField] private float wallPatternCooldown = 8.0f;
    [SerializeField] private float jumpSpeed = 10.0f;
    [SerializeField] private float slamSpeed = 15.0f;
    [SerializeField] private float clingDuration = 1.0f;
    [SerializeField] private Transform[] wallPoints;

    [Header("References")]
    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // ★ [수정 1] EnemyHealth 대신 BossController를 참조
    private BossController bossController; 

    // 상태 변수
    private bool isAttacking = false; 
    private bool isSpecialAttacking = false; 
    private float nextAttackTime = 0f;
    private float nextWallPatternTime = 0f;
    private Vector3 originalScale;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        // ★ [수정 2] 같은 오브젝트에 있는 BossController 가져오기
        bossController = GetComponent<BossController>();
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        // 시작하자마자 패턴 안 나오게 쿨타임 주기
        nextWallPatternTime = Time.time + 3.0f;
    }

    private void Update()
    {
        if (player == null || isSpecialAttacking) return;
        
        // ★ [추가] 보스가 죽었으면(HP 0) 패턴 중지
        if (bossController != null && bossController.GetCurrentHealth() <= 0) return;

        Vector2 centerPos = (Vector2)transform.position + rangeCenterOffset;
        float distanceToPlayer = Vector2.Distance(centerPos, player.position);

        // ====================================================
        // ★ [수정 3] BossController에게 체력 비율 물어보기
        // ====================================================
        bool isPhaseTwo = false;
        if (bossController != null)
        {
            // 방금 만든 함수 호출
            if (bossController.GetHealthPercentage() <= phaseTwoRatio)
            {
                isPhaseTwo = true;
            }
        }

        // 2페이즈 진입 & 쿨타임 완료 시 특수 패턴
        if (isPhaseTwo && Time.time >= nextWallPatternTime)
        {
            StartCoroutine(WallSlamRoutine());
            return; 
        }

        // --- 기본 추적 및 공격 로직 ---
        if (distanceToPlayer <= attackRange)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer <= detectRange && !isAttacking)
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }
    }

    // ... (WallSlamRoutine, AttackRoutine 등 나머지 로직은 완벽히 동일) ...
    
    // ★ [수정 4] 데미지 주는 함수 (BossController도 TakeDamage가 있으니 그거 써도 되지만, 
    // 여기서는 플레이어를 때리는 거니까 그대로 둠)
    private void DealAreaDamage(float range)
    {
        Vector2 centerPos = (Vector2)transform.position + rangeCenterOffset;
        if (Vector2.Distance(centerPos, player.position) <= range)
        {
            player.GetComponent<PlayerStats>()?.TakeDamage(attackDamage, transform);
        }
    }
    
    // ... (나머지 이동/애니메이션 함수들 그대로 유지) ...
    
    private IEnumerator WallSlamRoutine()
    {
        isSpecialAttacking = true; 
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; 
        anim.SetTrigger("Jump"); 

        Transform targetWall = wallPoints[Random.Range(0, wallPoints.Length)];
        
        // 벽으로 이동
        while (Vector2.Distance(transform.position, targetWall.position) > 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetWall.position, jumpSpeed * Time.deltaTime);
            yield return null;
        }

        anim.SetBool("WallCling", true);
        FlipTowards(player.position);
        yield return new WaitForSeconds(clingDuration);

        Vector3 slamTarget = player.position;
        anim.SetBool("WallCling", false);
        anim.SetTrigger("Slam"); 

        // 내려찍기
        while (Vector2.Distance(transform.position, slamTarget) > 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, slamTarget, slamSpeed * Time.deltaTime);
            yield return null;
        }

        DealAreaDamage(attackRange * 2.0f); 

        yield return new WaitForSeconds(attackAfterDelay);

        rb.bodyType = RigidbodyType2D.Dynamic; 
        nextWallPatternTime = Time.time + wallPatternCooldown; 
        isSpecialAttacking = false; 
    }

    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        anim.SetBool("Walk", true);
        FlipTowards(player.position);
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("Walk", false);
    }

    private void FlipTowards(Vector3 target)
    {
        if (target.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
    }

    private void AttackPlayer()
    {
        StopMoving();
        if (Time.time >= nextAttackTime && !isAttacking)
        {
            isAttacking = true;
            nextAttackTime = Time.time + attackRate;
            anim.SetTrigger("Attack1");
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(0.2f); 
        DealAreaDamage(attackRange);           
        yield return new WaitForSeconds(attackAfterDelay); 
        isAttacking = false;
    }
    
    private void OnDrawGizmosSelected()
    {
        Vector2 centerPos = (Vector2)transform.position + rangeCenterOffset;
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(centerPos, detectRange);
        Gizmos.color = Color.red;    Gizmos.DrawWireSphere(centerPos, attackRange);
        if(wallPoints != null) {
            Gizmos.color = Color.cyan;
            foreach(var wall in wallPoints) if(wall != null) Gizmos.DrawWireSphere(wall.position, 0.5f);
        }
    }
}