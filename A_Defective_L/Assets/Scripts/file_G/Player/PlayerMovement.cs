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

    [SerializeField] private float sprintSpeed = 8f; // 달리기 속도 

    [Header("Dash (Evasion) Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashInvincibility = 0.2f;
    [SerializeField] private float dashCooldown = 1.0f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    // 벽 관련 설정
    [Header("Wall Detection & Movement")]
    [SerializeField] private LayerMask wallLayer;       // 벽 레이어
    [SerializeField] private Transform wallCheck;       // 벽 감지 위치 (오브젝트에 태그 부착 필요)
    [SerializeField] private float wallCheckRadius = 0.2f;
    [SerializeField] private float wallSlideSpeed = 2f; // 벽 타고 내려오는 속도

    [Header("Wall Jump Settings")]
  private bool isWallJumping; // 벽 점프 중인지 확인
private float wallJumpDirection; // 튕겨나가는 방향
private float wallJumpTime = 0.2f; // 방향키 무시할 시간
private float wallJumpCounter; // 시간 계산용
[SerializeField] private Vector2 wallJumpPower = new Vector2(8f, 16f); // 점프 힘
    
    [Header("Effects")]
    public GameObject dodgeEffectPrefab; // 회피 이펙트 프리팹 연결
    public Vector2 dodgeEffectOffset;    // 위치 미세 조정용 
    
    [Header("Audio")]
    public AudioClip footstepSound;

    public AudioClip dashSound; // 대시 사운드 인스펙터에서 연결
    public float footstepRate = 0.4f; // 0.4초마다 발소리 재생
    private float nextFootstepTime;
    public float SprintStepRate = 0.2f;

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
    private float defaultGravity; // 중력값 저장용

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<PlayerStats>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        defaultGravity = rb.gravityScale; // 시작할 때 설정된 중력값을 기본값으로 기억

        // 부활하자마자 발소리가 터지는 것을 방지하기 위해 재생 시간을 0.1초 뒤로 밀어둠.
    nextFootstepTime = Time.time + 0.1f;
    }

    private void Update()
    {
        // 컷신, 시간 정지, 대시, 넉백 중에는 입력 차단
        if (GameManager.Instance != null && GameManager.Instance.IsCutscene) return;
        if (Time.timeScale == 0) return;
        if (isDashing || isKnockback) return;

        // 입력 감지
        horizontalInput = Input.GetAxisRaw("Horizontal");

       // 발소리 재생 부분 
       if (isGrounded && Mathf.Abs(horizontalInput) > 0.1f)
        {
            // 현재 달리기(Sprint) 상태인지 확인
            // 달리기 능력 보유 + C키 누름
            bool isSprinting = DataManager.Instance.currentData.hasSprint && Input.GetKey(KeyCode.C);

            // 달리면 0.2초(빠름), 걸으면 0.4초(보통)
            float currentStepRate = isSprinting ? SprintStepRate : footstepRate;

            // 시간 체크 및 소리 재생
            if (Time.time >= nextFootstepTime)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(footstepSound);
                }
                
                // 다음 발소리 시간 갱신
                nextFootstepTime = Time.time + currentStepRate;
            }
        }
        else
        {
            // 멈춰있을 때는 즉시 소리가 날 준비를 함
            nextFootstepTime = 0f;
        }
        // 점프 (땅에 있을 때만)
        if (Input.GetKeyDown(KeyCode.Space))
{
    // 땅에 있으면 일반 점프
    if (isGrounded)
    {
        Jump();
        anim.SetBool("IsJump", true);
    }
    // 땅은 아닌데 벽에 붙어있으면 벽 점프
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

        // 회피 돌진 (C키)
        if (Input.GetKeyDown(KeyCode.C) && canDash)
        {
            StartCoroutine(DashProcess());
            // 대시 애니메이션 처리 
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

        // 애니메이션 및 방향 전환
        // 벽에 매달려 있을 때는 방향 전환 금지 
        if (!isWallClinging) 
        {
            if (horizontalInput == 0) anim.SetBool("IsRun", false);
            else
            {
                anim.SetBool("IsRun", true);
                FlipCharacter();
            }
        }
        
        // 벽 애니메이션 업데이트
        UpdateWallAnimation();
    }

    private void FixedUpdate()
    {
        if (isDashing || isKnockback) return;

        CheckGround(); // 땅 체크
        CheckWall();   // 벽 체크 추가

        // 벽 점프 중이면 아무것도 안 함 (벽 점프 로직이 자체적으로 이동과 중력 제어를 하기 때문)
    if (isWallJumping) return;
    
        // 벽 타기 로직이 먼저 계산되어야 함 (중력을 제어 문제)
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
        float currentSpeed = moveSpeed;

        // 질주 능력을 배웠고 (hasSprint) C키를 누르고 있고 (GetKey) 땅에 있을 때만 질주 속도 적용
        if (DataManager.Instance.currentData.hasSprint && Input.GetKey(KeyCode.C))
        {
            currentSpeed = sprintSpeed;
            
        }
        // 실제 이동 적용
        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);   
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // 벽 타기 및 슬라이딩 로직
    private bool CheckWallInteraction()
    {
        // 땅에 있거나 벽이 없으면 벽 타기 안 함
        if (isGrounded || !isTouchingWall)
        {
            isWallClinging = false;
            return false;
        }

        // 벽 쪽으로 방향키를 누르고 있는지 확인
        //오른쪽 보고 있을 때 오른쪽 키, 왼쪽 보고 있을 때 왼쪽 키
        bool isPushingWall = (isFacingRight && horizontalInput > 0) || (!isFacingRight && horizontalInput < 0);

        if (isPushingWall)
        {
            //매달리기 키를 꾹 누르면 -> 멈춤
            isWallClinging = true;
            rb.gravityScale = 0; 
            rb.linearVelocity = Vector2.zero; 
        }
        else
        {
            // 키를 안 누르면 -> 천천히 내려감 ... 작동 문제로 슬라이드는 적용은 안됨
            isWallClinging = false;
            
            // 떨어지는 중일 때만 속도 제어 
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
        
        // 슬라이딩 애니메이션 벽엔 닿고, 땅은 아니며, 매달린 건 아닌 경우)
        anim.SetBool("IsWallSliding", isTouchingWall && !isGrounded && !isWallClinging);
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // 벽 감지 함수
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

    // 대시넉백

    private IEnumerator DashProcess()
    {
        canDash = false;
        isDashing = true;

        // 대시 시작하자마자 소리 1번 재생
        if (AudioManager.Instance != null && dashSound != null)
        {
            AudioManager.Instance.PlaySFX(dashSound); 
        }

        // 회피 이펙트 생성 로직
        if (dodgeEffectPrefab != null)
        {
            // 위치 설정: 플레이어 위치 + 오프셋
            // (참고: 발밑 먼지라면 offset y를 조금 내리세요)
            Vector3 spawnPos = transform.position + (Vector3)dodgeEffectOffset;

            // 생성
            GameObject effect = Instantiate(dodgeEffectPrefab, spawnPos, Quaternion.identity);

            // 좌우 반전 (플레이어가 보는 방향에 맞춤)
            // 플레이어의 scale.x가 -1이면 이펙트도 -1로 뒤집기
            Vector3 playerScale = transform.localScale;
            Vector3 effectScale = effect.transform.localScale;

            // 방향 맞추기 (오른쪽: 1, 왼쪽: -1)
            effectScale.x = Mathf.Abs(effectScale.x) * Mathf.Sign(playerScale.x);
            
            effect.transform.localScale = effectScale;
        }

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

    // 잠시 조작 불능 상태로 만듦 (Move 함수 차단용)
    StartCoroutine(WallJumpCoroutine());
}

// 0.2초 동안 조작을 막고 튕겨나가는 힘을 유지하는 코루틴
private IEnumerator WallJumpCoroutine()
{
    isWallJumping = true; // 이동 막기 시작
    
    // 힘 가하기 (기존 속도 리셋 후 적용)
    rb.linearVelocity = Vector2.zero; 
    rb.AddForce(new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y), ForceMode2D.Impulse);

    // 캐릭터 뒤집기
    if (transform.localScale.x != wallJumpDirection)
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // 0.2초 대기 (이 시간 동안 방향키가 안 먹혀서 벽에서 멀어질 수 있음)
    yield return new WaitForSeconds(wallJumpTime);

    isWallJumping = false; // 이동 허용
}

// 승강기에서 호출
    public void StopImmediately()
    {

        if (GetComponent<Rigidbody2D>() != null)
        {
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
       

    }
}