using System.Collections;
using UnityEngine;

public class BossWallPattern : MonoBehaviour //벽타는 보스. 지정된 오브젝트 포인트로 이동하고 내려찍는 패턴으로 구성
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float detectRange = 8.0f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private int attackDamage = 1;

    [Header("Settings")]
    [SerializeField] private float attackRate = 1.5f;
    [SerializeField] private float attackAfterDelay = 1.0f;
    [SerializeField] private Vector2 rangeCenterOffset;

    [Header("Wall Pattern")]
    [SerializeField] private float phaseTwoRatio = 0.5f; // 체력 50퍼
    [SerializeField] private float wallPatternCooldown = 8.0f;
    [SerializeField] private float jumpSpeed = 10.0f;
    [SerializeField] private float slamSpeed = 15.0f;
    [SerializeField] private float clingDuration = 1.0f;
    [SerializeField] private Transform[] wallPoints;

    // 내려찍기 속도
    [SerializeField] private float slamHorizontalForce = 8.0f; // 플레이어 쪽으로 쫓아가는 X축 힘
    [SerializeField] private float slamDownwardForce = 25.0f;  // 아래로 내리꽂는 Y축 중력 가속도

    [Header("Hitboxes")]
    // 자식 오브젝트로 내려찍기 전용 히트박스 연결
    [SerializeField] private GameObject slamHitbox;

    [Header("References")]
    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // BossController를 참조
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

        // 같은 오브젝트에 있는 BossController 가져옴
        bossController = GetComponent<BossController>();
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        
        // 시작하자마자 패턴 안 나오게 쿨타임 줌
        nextWallPatternTime = Time.time + 3.0f;
    }

    private void Update()
    {
        if (player == null || isSpecialAttacking) return;
        
        // 보스 HP가 0이면 패턴 중지
        if (bossController != null && bossController.GetCurrentHealth() <= 0) return;

        Vector2 centerPos = (Vector2)transform.position + rangeCenterOffset;
        float distanceToPlayer = Vector2.Distance(centerPos, player.position);

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

        //기본 추적 및 공격 로직 
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

    private void DealAreaDamage(float range)
    {
        Vector2 centerPos = (Vector2)transform.position + rangeCenterOffset;
        if (Vector2.Distance(centerPos, player.position) <= range)
        {
            player.GetComponent<PlayerStats>()?.TakeDamage(attackDamage, transform);
        }
    }
    
    private IEnumerator WallSlamRoutine()
    {
        isSpecialAttacking = true; 
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // 올라갈 땐 물리 끔, 중력 영향을 받으면 점프가 정상적으로 안 되는 경우 때문
           anim.SetBool("Jump",true);

        Transform targetWall = wallPoints[Random.Range(0, wallPoints.Length)];
        
        // 벽으로 점프 
        while (Vector2.Distance(transform.position, targetWall.position) > 0.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetWall.position, jumpSpeed * Time.deltaTime);
            yield return null;
        }

        // 벽에 매달리기
        FlipTowards(player.position);
        yield return new WaitForSeconds(clingDuration);

        // 내려찍기
        anim.SetBool("Jump", false);

        // 물리를 다시 켜서 중력의 영향을 받게 함
        rb.bodyType = RigidbodyType2D.Dynamic; 

        //플레이어 방향 계산 X축 방향
        float dirX = (player.position.x > transform.position.x) ? 1f : -1f;
        
        // 대각선 아래로 강력하게 내려찍음 (중력 + 강제 하강 속도)
        rb.linearVelocity = new Vector2(dirX * slamHorizontalForce, -slamDownwardForce);

        // 내려찍기 전용 히트박스를 켜서 해당 범위에 있는 경우 플레이어는 공격 히트박스에 있는 Enemy 태그에 데미지를 받게 됨
        if (slamHitbox != null) slamHitbox.SetActive(true);

        // 0.1초 대기 
        yield return new WaitForSeconds(0.1f);
        anim.SetTrigger("DashGo"); 
        // 바닥에 닿았는지 확인
        // 공중에서 떨어지는 중이면 velocity.y는 마이너스 값
        while (rb.linearVelocity.y < -0.1f)
        {
            yield return null;
        }

        // 바닥에 닿음 
        rb.linearVelocity = Vector2.zero; // 착지 후 미끄러짐 방지
        
        // 히트박스 끔
        if (slamHitbox != null) slamHitbox.SetActive(false);

        yield return new WaitForSeconds(attackAfterDelay);

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