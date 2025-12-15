using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Stats")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;

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
    private float horizontalInput;
    private bool isFacingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (isDashing) return; 

        // 1. 입력 감지
        horizontalInput = Input.GetAxisRaw("Horizontal"); 
        
        // 2. 점프 (Z키)
        if (Input.GetKeyDown(KeyCode.Z) && isGrounded)
        {
            Jump();
        }

        // 3. 회피 돌진 (C키)
        if (Input.GetKeyDown(KeyCode.C) && canDash)
        {
            StartCoroutine(DashProcess());
        }

        // 4. 캐릭터 좌우 반전
        FlipCharacter();
    }

    private void FixedUpdate()
    {
        if (isDashing) return; 

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
}