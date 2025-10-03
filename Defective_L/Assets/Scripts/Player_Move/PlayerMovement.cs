// PlayerMovement.cs

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 7f;
    public bool isFacingRight { get; private set; } // 현재 오른쪽을 보는지 여부

    private Rigidbody2D rb;
    private PlayerDash playerDash; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerDash = GetComponent<PlayerDash>();
        isFacingRight = true; // 시작할 때 오른쪽을 보도록 설정
    }

    public void Move(float moveInput)
    {
        if (playerDash != null && playerDash.isDashing)
        {
            return;
        }
        
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // --- 방향 전환 로직 추가 ---
        if (moveInput > 0 && !isFacingRight)
        {
            Flip(); // 오른쪽으로 이동하는데 왼쪽을 보고 있다면 뒤집기
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip(); // 왼쪽으로 이동하는데 오른쪽을 보고 있다면 뒤집기
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight; // 방향 상태를 반전
        Vector3 theScale = transform.localScale;
        theScale.x *= -1; // x축 스케일을 반전시켜 그래픽을 뒤집음
        transform.localScale = theScale;
    }
}