using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Stats")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Dash (Evasion) Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashInvincibility = 0.2f;
    [SerializeField] private float dashCooldown = 1.0f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    // ★ [추가] 벽 관련 설정
    [Header("Wall Detection & Movement")]
    [SerializeField] private LayerMask wallLayer;       // 벽 레이어
    [SerializeField] private Transform wallCheck;       // 벽 감지 위치 (오브젝트 필요)
    [SerializeField] private float wallCheckRadius = 0.2f;
    [SerializeField] private float wallSlideSpeed = 2f; // 벽 타고 내려오는 속도

    [Header("Wall Jump Settings")]
  private bool isWallJumping; // 벽 점프 중인지 확인
private float wallJumpDirection; // (선택) 튕겨나가는 방향
private float wallJumpTime = 0.2f; // 방향키 무시할 시간
private float wallJumpCounter; // 시간 계산용
[SerializeField] private Vector2 wallJumpPower = new Vector2(8f, 16f); // 점프 파워
    
    // 내부 변수
    private Rigidbody2D rb;
    private PlayerStats playerStats;
    private Animator anim;

    // 상태 변수
    private bool isGrounded;
    private bool isTouchingWall; // 벽에 닿았는지
    private bool isWallClinging; // 매달려 있는지
    private bool isDashing;
    private bool canDash = true;
    private bool isKnockback = false;

    private float horizontalInput;
    private bool isFacingRight = true;
    private float defaultGravity; // 원래 중력값 저장용

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<PlayerStats>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        defaultGravity = rb.gravityScale; // 시작할 때 설정된 중력값을 기본값으로 기억
    }

    private void Update()
    {
        // 컷신, 시간 정지, 대시, 넉백 중에는 입력 차단
        if (GameManager.Instance != null && GameManager.Instance.IsCutscene) return;
        if (Time.timeScale == 0) return;
        if (isDashing || isKnockback) return;

        // 1. 입력 감지
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. 점프 (땅에 있을 때만)
        // (나중에 벽 점프를 추가하려면 여기에 || isTouchingWall 조건 추가 필요)
        if (Input.GetKeyDown(KeyCode.Space))
{
    // 1. 땅에 있으면 -> 일반 점프
    if (isGrounded)
    {
        Jump();
        anim.SetBool("IsJump", true);
    }
    // 2. 땅은 아닌데 벽에 붙어있으면 -> 벽 점프!
    else if (isTouchingWall)
    {
        WallJump(); // 새로 만들 함수
        anim.SetBool("IsJump", true); // 점프 모션 재생
    }
}
        
        else
        {
            // 땅에 닿으면 점프 애니메이션 해제 (벽 타기 중에도 점프 모션 꺼짐 방지)
            if(isGrounded) anim.SetBool("IsJump", false);
        }

        // 3. 회피 돌진 (C키)
        if (Input.GetKeyDown(KeyCode.C) && canDash)
        {
            StartCoroutine(DashProcess());
            // 대시 애니메이션 처리 (기존 유지)
            if (anim.GetBool("IsRun")) anim.SetBool("Dash_R", true);
            else anim.SetBool("Dash_I", true);
        }
        else if (anim.GetBool("Dash_R"))
        {
            anim.SetBool("Dash_R", false);
        }
        else
        {
            anim.SetBool("Dash_I", false);
        }

        // 4. 애니메이션 및 방향 전환
        // (벽에 매달려 있을 때는 방향 전환 금지 - 어색함 방지)
        if (!isWallClinging) 
        {
            if (horizontalInput == 0) anim.SetBool("IsRun", false);
            else
            {
                anim.SetBool("IsRun", true);
                FlipCharacter();
            }
        }
        
        // ★ [추가] 벽 애니메이션 업데이트
        UpdateWallAnimation();
    }

    private void FixedUpdate()
    {
        if (isDashing || isKnockback) return;

        CheckGround(); // 땅 체크
        CheckWall();   // ★ 벽 체크 추가

        // ★ [수정] 벽 점프 중이면 아무것도 안 함 (힘 보존)
    if (isWallJumping) return;
    
        // ★ 벽 타기 로직이 먼저 계산되어야 함 (중력을 제어하므로)
        if (CheckWallInteraction()) 
        {
            // 벽 타기 중이면 Move()를 호출하지 않거나 제한적으로 호출
            // 여기서는 벽 타기 중에는 좌우 이동을 막고 벽 로직만 따름
        }
        else
        {
            Move(); // 평소 이동
            rb.gravityScale = defaultGravity; // 벽에서 떨어지면 중력 복구
        }
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // ★ [핵심] 벽 타기 및 슬라이딩 로직
    private bool CheckWallInteraction()
    {
        // 땅에 있거나 벽이 없으면 벽 타기 안 함
        if (isGrounded || !isTouchingWall)
        {
            isWallClinging = false;
            return false;
        }

        // 벽 쪽으로 방향키를 누르고 있는지 확인
        // (오른쪽 보고 있을 때 오른쪽 키 OR 왼쪽 보고 있을 때 왼쪽 키)
        bool isPushingWall = (isFacingRight && horizontalInput > 0) || (!isFacingRight && horizontalInput < 0);

        if (isPushingWall)
        {
            // [매달리기] 키를 꾹 누르면 -> 멈춤
            isWallClinging = true;
            rb.gravityScale = 0; 
            rb.linearVelocity = Vector2.zero; 
        }
        else
        {
            // [슬라이딩] 키를 안 누르면 -> 천천히 내려감
            isWallClinging = false;
            
            // 떨어지는 중일 때만 속도 제어 (올라갈 땐 제어 X)
            if (rb.linearVelocity.y < 0)
            {
                rb.gravityScale = wallSlideSpeed; 
            }
        }

        return true; // 벽 상호작용 중임
    }

    private void UpdateWallAnimation()
    {
        // 매달리기 애니메이션
        anim.SetBool("IsWallClinging", isWallClinging);
        
        // 슬라이딩 애니메이션 (벽엔 닿았는데, 땅은 아니고, 매달린 건 아닐 때)
        anim.SetBool("IsWallSliding", isTouchingWall && !isGrounded && !isWallClinging);
    }

    // --- 유틸리티 ---

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // ★ 벽 감지 함수
    private void CheckWall()
    {
        if (wallCheck != null)
            isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
    }

    private void FlipCharacter()
    {
        if (horizontalInput > 0 && !isFacingRight) Flip();
        else if (horizontalInput < 0 && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // --- 대시 & 넉백 (기존 유지) ---

    private IEnumerator DashProcess()
    {
        canDash = false;
        isDashing = true;

        float dashDirection = horizontalInput == 0 ? (isFacingRight ? 1 : -1) : horizontalInput;

        if (playerStats != null) playerStats.SetInvincible(dashInvincibility);

        // 대시 중에는 중력 0
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        // 대시 끝나면 원래 중력으로 복구 (defaultGravity 사용)
        rb.gravityScale = defaultGravity; 
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void ApplyKnockback(Transform damageSource)
    {
        if (isKnockback) return;
        StartCoroutine(KnockbackRoutine(damageSource));
    }

    private IEnumerator KnockbackRoutine(Transform damageSource)
    {
        isKnockback = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = defaultGravity; // 넉백될 때는 정상 중력 적용

        Vector2 direction = (transform.position - damageSource.position).normalized;
        Vector2 knockbackDir = new Vector2(direction.x, 0.5f).normalized;

        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        
        yield return new WaitForSeconds(knockbackDuration);
        isKnockback = false;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        // 벽 감지 범위 표시
        if (wallCheck)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }

   private void WallJump()
{
    isWallClinging = false;
    rb.gravityScale = defaultGravity;
    
    // 벽 반대 방향 구하기
    wallJumpDirection = -transform.localScale.x; 

    // 1. 잠시 조작 불능 상태로 만듦 (Move 함수 차단용)
    StartCoroutine(WallJumpCoroutine());
}

// 0.2초 동안 조작을 막고 튕겨나가는 힘을 유지하는 코루틴
private IEnumerator WallJumpCoroutine()
{
    isWallJumping = true; // 이동 막기 시작
    
    // 2. 힘 가하기 (기존 속도 리셋 후 적용)
    rb.linearVelocity = Vector2.zero; 
    rb.AddForce(new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y), ForceMode2D.Impulse);

    // 3. 캐릭터 뒤집기
    if (transform.localScale.x != wallJumpDirection)
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // 4. 0.2초 대기 (이 시간 동안은 방향키가 안 먹힘 -> 벽에서 멀어질 수 있음)
    yield return new WaitForSeconds(wallJumpTime);

    isWallJumping = false; // 이동 허용
}
}