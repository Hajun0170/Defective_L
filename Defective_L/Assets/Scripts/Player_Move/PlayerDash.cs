// PlayerDash.cs

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    public bool isDashing { get; private set; } 

    private Rigidbody2D rb;
    private bool canDash = true;
    private PlayerMovement playerMovement; // PlayerMovement를 참조할 변수 추가

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>(); // 컴포넌트 가져오기
        isDashing = false;
    }

   // PlayerController가 호출할 수 있도록 public으로 선언된 함수
    public void PerformDash()
    {
        if (canDash)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true; 

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // PlayerMovement의 isFacingRight 값을 사용해 대쉬 방향 결정
        float dashDirection = playerMovement.isFacingRight ? 1 : -1;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero; 
        isDashing = false; 

        yield return new WaitForSeconds(dashCooldown);
        canDash = true; 
    }
}