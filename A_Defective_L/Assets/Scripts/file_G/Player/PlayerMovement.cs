using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Stats")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float knockbackForce = 10f; // 넉백 파워
    [SerializeField] private float knockbackDuration = 0.2f; // 밀려나는 시간

    [Header("Dash (Evasion) Settings")]
    [SerializeField] private float dashSpeed = 20f;      
    [SerializeField] private float dashDuration = 0.15f; 
    [SerializeField] private float dashInvincibility = 0.2f; 
    [SerializeField] private float dashCooldown = 1.0f;  

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;      
    [SerializeField] private float groundCheckRadius = 0.2f;

    // 내부 변수
    private Rigidbody2D rb;
    private PlayerStats playerStats;
    private bool isGrounded;
    private bool isDashing;
    private bool canDash = true;

    // [추가] 넉백 중인지 확인하는 변수
    private bool isKnockback = false;
    private float horizontalInput;
    private bool isFacingRight = true;

    private Animator anim;  // 애니메이터 변수

    void Start()
    {
        anim = GetComponent<Animator>(); // 내 몸에 붙은 애니메이터 가져오기
        rb = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        // [추가] 컷신 중이거나 시간이 멈췄으면 조작 불가
        if (GameManager.Instance != null && GameManager.Instance.IsCutscene) return;
        if (Time.timeScale == 0) return;

        // [수정] 대시 중이거나 "넉백 중"이면 입력 무시
        if (isDashing || isKnockback) return;

        if (isDashing) return; 

        // 1. 입력 감지
        horizontalInput = Input.GetAxisRaw("Horizontal"); //wasd 입력 방지 필요.
        
        // 2. 점프 (스페이스바)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
            anim.SetBool("IsJump", true);
        }
        else
        {
            anim.SetBool("IsJump", false);
        }

        // 3. 회피 돌진 (C키)
        if (Input.GetKeyDown(KeyCode.C) && canDash)
        {
            StartCoroutine(DashProcess());

            if(anim.GetBool("IsRun") == true)
            {
                anim.SetBool("Dash_R", true);
            }
            else
            {
                anim.SetBool("Dash_I", true);
            }
        }
        else if (anim.GetBool("Dash_R") == true)
        {
            anim.SetBool("Dash_R", false);
        }
        else
        {
            anim.SetBool("Dash_I", false);
        }


        // 3. 애니메이션 제어
        if (horizontalInput == 0) 
        {
            // 멈춤 상태
            anim.SetBool("IsRun", false);
        }
        else
        {
            // 이동 중 -> IsRun 켜기
            anim.SetBool("IsRun", true);
            FlipCharacter();
        }
    }

    private void FixedUpdate()
    {
        // [수정] 대시 중이거나 "넉백 중"이면 이동 로직 실행 안 함 (물리 힘 보존)
        if (isDashing || isKnockback) return;

        CheckGround();
        Move();
    }

    private void Move()
    {     
        // Unity 6 최신 버전: velocity 대신 linearVelocity 사용
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // --- 대시(회피) 로직 ---
    private IEnumerator DashProcess()
    {
        canDash = false;
        isDashing = true;

        // 대시 방향 결정 
        float dashDirection = horizontalInput == 0 ? (isFacingRight ? 1 : -1) : horizontalInput;

        // 1. 무적 부여
        if (playerStats != null) playerStats.SetInvincible(dashInvincibility);

        // 2. 속도 증가
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f; 
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        // 3. 대시 유지
        yield return new WaitForSeconds(dashDuration);

        // 4. 대시 종료
        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero; 
        isDashing = false;

        // 5. 쿨타임 대기
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
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

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmos()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    // [신규 기능] 외부에서 호출할 넉백 함수
    public void ApplyKnockback(Transform damageSource)
    {
        if (isKnockback) return; // 이미 밀려나는 중이면 무시

        StartCoroutine(KnockbackRoutine(damageSource));
    }

    private IEnumerator KnockbackRoutine(Transform damageSource)
    {
        isKnockback = true;

        // 1. 현재 이동 속도 초기화 (관성 제거)
        rb.linearVelocity = Vector2.zero;

        // 2. 밀려날 방향 계산: (내 위치 - 적 위치).normalized = 적 반대 방향
        Vector2 direction = (transform.position - damageSource.position).normalized;
        
        // 3. 위로 살짝 뜨면서 뒤로 밀리게 (x축 + y축 약간)
        // y축을 조금 섞어줘야 바닥 마찰력 때문에 안 밀리는 현상을 막을 수 있습니다.
        Vector2 knockbackDir = new Vector2(direction.x, 0.5f).normalized; 

        // 4. 힘 가하기 (Impulse)
        rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        Debug.Log("으악! 넉백!");

        // 5. 제어 불능 시간 대기
        yield return new WaitForSeconds(knockbackDuration);

        isKnockback = false;
    }
}