// PlayerJump.cs

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 15f;
    
    // --- 땅 감지를 위한 변수들 추가 ---
    public Transform groundCheck;     // 땅 감지 위치 (GroundCheck 오브젝트를 연결)
    public LayerMask groundLayer;     // 땅으로 인식할 레이어 (Ground 레이어를 선택)
    public float groundCheckRadius = 0.2f; // 땅 감지 범위

    private Rigidbody2D rb;
    private bool isGrounded; // 땅에 닿아있는지 여부

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Physics2D.OverlapCircle을 사용해 groundCheck 위치에 groundLayer가 있는지 확인
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public void PerformJump()
    {
        // 땅에 있을 때만 점프가 가능하도록 수정
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            Debug.Log("점프!");
        }
        else
        {
            Debug.Log("공중에서는 점프할 수 없습니다.");
        }
    }

    private void OnDrawGizmosSelected()
{
    // groundCheck 변수가 할당되지 않았으면 실행하지 않음
    if (groundCheck == null) return;

    // 기즈모 색상을 노란색으로 설정
    Gizmos.color = Color.yellow;
    // groundCheck의 위치에 groundCheckRadius 크기의 원을 그림
    Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
}
}